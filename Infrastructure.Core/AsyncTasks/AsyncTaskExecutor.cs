using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using DocumentFormat.OpenXml.Wordprocessing;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MimeKit.Encodings;

using Newtonsoft.Json;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AsyncTasks
{
    public class AsyncTaskExecutorHostedService : TimerHostedService
    {
        public AsyncTaskExecutorHostedService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AsyncTaskExecutorHostedService> logger,
            IApplicationSettings applicationSettings,
            IHostApplicationLifetime appLifetime)
            : base(serviceScopeFactory, appLifetime, logger, applicationSettings)
        {
            BackgroundServiceInfoEnabled = false;
        }

        protected override async Task ExecuteInternalAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var asyncTaskExecutor
                = scope.ServiceProvider.GetService<AsyncTaskExecutor>();

            if (await asyncTaskExecutor.AnyNextAsyncTaskOperationAsync(cancellationToken) == false)
                return;

            var parallelTasks = new List<Task>
            {
                asyncTaskExecutor.ExecuteNextOperationsAsync(_hostedServicesConfiguration.BatchSize, cancellationToken),
                asyncTaskExecutor.Delete(DateTime.Now.Subtract(TimeSpan.FromDays(100))),
                asyncTaskExecutor.HandleTimeout()
            };

            await Task.WhenAll(parallelTasks);
        }
    }

    public class AsyncTaskExecutor : IScopedDependency
    {
        private readonly ILogger<AsyncTaskExecutor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDbContext _context;

        public AsyncTaskExecutor(IServiceProvider serviceProvider,
            IDbContext context,
            ILogger<AsyncTaskExecutor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _context = context;
        }

        private static readonly Dictionary<Guid, Type> _handlerTypes = CollectAllHandler();

        private static Dictionary<Guid, Type> CollectAllHandler()
        {
            var handlerTypes =
                    AssemblyUtils.AllLoadedTypes()
                    .Where(t => t.IsClass && !t.IsInterface && !t.IsAbstract &&
                                typeof(IAsyncTaskOperationHandler).IsAssignableFrom(t))
                    .ToDictionary(t =>
                            t.GetCustomAttribute<OperationHandlerAttribute>().Id,
                        t => t);

            return handlerTypes;
        }

        private SemaphoreSlim _semaphore = null;

        public async Task ExecuteNextOperationsAsync(int batchSize = 10, CancellationToken cancellationToken = default)
        {
            if (_semaphore == null)
                _semaphore = new SemaphoreSlim(batchSize, batchSize);

            var tasks = new List<Task>();

            while (await AnyNextAsyncTaskOperationAsync(cancellationToken) && tasks.Count < 1000)
            {
                await _semaphore.WaitAsync(cancellationToken);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var asyncTaskExecutor = scope.ServiceProvider.GetService<AsyncTaskExecutor>();

                            await asyncTaskExecutor.ExecuteNextOperationAsync(cancellationToken);
                        }
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            while (IsAnyTasksRunning(tasks))
            {
                await ExecuteNextOperationsAsync(batchSize, cancellationToken);
                
                await Task.Delay(100);
            }

            await Task.WhenAll(tasks);
        }

        public bool IsAnyTasksRunning(IEnumerable<Task> tasks)
        {
            return tasks.Any(t => !(t.IsCompleted || t.IsCanceled || t.IsFaulted));
        }

        private async Task ExecuteNextOperationAsync(CancellationToken cancellationToken = default)
        {
            var dateNow = DateTime.Now;
            Guid asyncTaskOperationId = Guid.Empty;

            using (var distributedLock = _serviceProvider.GetRequiredService<IDistributedLock>())
            {
                var lockName = $"{nameof(AsyncTaskExecutor)}_{nameof(ExecuteNextOperationAsync)}";
                if (await distributedLock.TryAcquireLockAsync(lockName, 1000) == false)
                    throw new TimeoutException();

                await RemoveDuplicatesAsync(dateNow, cancellationToken);

                var nextIds = await GetNextAsyncTaskOperationIdsAsync(dateNow, 1, cancellationToken);

                if (nextIds.Any() == false)
                    return;

                asyncTaskOperationId = nextIds.Single();

                await SetExecutingAsync(asyncTaskOperationId);
            }

            using (var childScope = _serviceProvider.CreateScope())
            {
                var asyncTaskExecutor = childScope.ServiceProvider.GetService<AsyncTaskExecutor>();

                await asyncTaskExecutor.ExecuteAsyncTaskOperationIdAsync(asyncTaskOperationId, cancellationToken);
            }
        }

        public async Task<bool> ExecuteAsyncTaskOperationIdAsync(Guid asyncTaskOperationId, CancellationToken cancellationToken)
        {
            using (var distributedLock = _serviceProvider.GetRequiredService<IDistributedLock>())
            {
                var lockName = $"{nameof(AsyncTaskExecutor)}_{nameof(ExecuteAsyncTaskOperationIdAsync)}_{asyncTaskOperationId}";
                if (distributedLock.TryAcquireLock(lockName, 3) == false)
                    throw new InvalidOperationException();

                try
                {
                    var asyncTaskOperation = await _context.Set<AsyncTaskOperation>()
                        .SingleOrDefaultAsync(o => o.Id == asyncTaskOperationId);

                    if (asyncTaskOperation.Status > AsyncTaskOperationStatus.Executing)
                        throw new InvalidOperationException();

                    if (asyncTaskOperation.ExecuteById.HasValue)
                    {
                        _serviceProvider.GetService<ICurrentUserService>()
                            .SetCurrentUserId(asyncTaskOperation.ExecuteById.Value);
                    }

                    var handlerType = _handlerTypes[asyncTaskOperation.OperationHandlerId];

                    var asyncTaskOperationHandler = (IAsyncTaskOperationHandler)_serviceProvider.GetService(handlerType);

                    JsonConvert.PopulateObject(asyncTaskOperation.ParameterSerialized,
                        asyncTaskOperationHandler, null);

                    await SetExecutingAsync(asyncTaskOperationId);

                    await asyncTaskOperationHandler.ExecuteAsync(cancellationToken);

                    await SetSuccessAsync(asyncTaskOperationId);

                    return true;
                }
                //catch (DbUpdateConcurrencyException ex)
                //{
                //    HandleError(asyncTaskOperation, ex);
                //    return;
                //}
                catch (Exception ex)
                {
                    await HandleErrorAsync(asyncTaskOperationId, ex);


#if DEBUG
                    Debugger.Break();
#endif

                    return false;
                }
            }
        }

        private async Task SetExecutingAsync(params Guid[] asyncTaskOperationIds)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<IDbContext>();

                var asyncTaskOperations = await context.Set<AsyncTaskOperation>()
                    .Where(_ => asyncTaskOperationIds.Contains(_.Id))
                    .ToListAsync();

                foreach (var asyncTaskOperation in asyncTaskOperations)
                {
                    if (asyncTaskOperation.Status != AsyncTaskOperationStatus.Executing)
                    {
                        asyncTaskOperation.Status = AsyncTaskOperationStatus.Executing;
                        asyncTaskOperation.StartedAt = DateTime.Now;
                    }
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task SetSuccessAsync(Guid asyncTaskOperationId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<IDbContext>();

                var asyncTaskOperation = await context.Set<AsyncTaskOperation>()
                    .SingleOrDefaultAsync(o => o.Id == asyncTaskOperationId);

                asyncTaskOperation.Status = AsyncTaskOperationStatus.Success;
                asyncTaskOperation.FinishedAt = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }

        private async Task HandleErrorAsync(Guid asyncTaskOperationId, Exception ex)
        {
            try
            {
                _logger.LogError(ex, "Fehler beim Verarbeiten der AsyncOperation: " + asyncTaskOperationId);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<IDbContext>();

                    var asyncTaskOperation = await context.Set<AsyncTaskOperation>()
                        .SingleOrDefaultAsync(o => o.Id == asyncTaskOperationId);

                    if (asyncTaskOperation.RetryCount < asyncTaskOperation.MaxRetryCount)
                    {
                        asyncTaskOperation.Status = AsyncTaskOperationStatus.Pending;
                        asyncTaskOperation.StartedAt = null;
                        asyncTaskOperation.ExecuteAt = DateTime.Now.Add(TimeSpan.FromSeconds(asyncTaskOperation.RetryCount * 5));
                        asyncTaskOperation.RetryCount = asyncTaskOperation.RetryCount + 1;
                        asyncTaskOperation.ErrorMessage = ex.ToString();
                    }
                    else
                    {
                        asyncTaskOperation.ExecuteAt = null;
                        asyncTaskOperation.Status = AsyncTaskOperationStatus.Failed;
                        asyncTaskOperation.ErrorMessage = ex.ToString();
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex2)
            {
                _logger.LogError(ex2, "Fehler HandleErrorAsync: " + asyncTaskOperationId);
            }
        }

        private async Task RemoveDuplicatesAsync(DateTime dateNow, CancellationToken cancellationToken)
        {
            var query = GetPendingQuery(dateNow)
                  .GroupBy(_ => _.OperationKey)
                  .Where(_ => _.Count() > 1)
                  .Select(_ => _.Key);

            if (await query.AnyAsync() == false)
                return;

            var operationKeys = await query.ToListAsync(cancellationToken);

            foreach (var operationKey in operationKeys)
            {
                var operations = await
                    GetPendingQuery(dateNow)
                    .Where(_ => _.OperationKey == operationKey)
                    .OrderBy(_ => _.ExecuteAt)
                    .Skip(1)
                    .ToListAsync(cancellationToken);

                operations.ForEach(_ =>
                    _.Status = AsyncTaskOperationStatus.Duplicate);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        //private const int _waitingExecuteToRetryInSec = 120;
        private async Task<Guid[]> GetNextAsyncTaskOperationIdsAsync(DateTime dateNow, int take = 1, CancellationToken cancellationToken = default)
        {
            var operationIds = await
                GetPendingQuery(dateNow)
                .OrderByDescending(_ => _.Priority)
                .ThenBy(_ => _.ExecuteAt)
                .ThenBy(_ => _.SortIndex)
                .Select(_ => _.Id)
                .Take(take)
                .ToArrayAsync(cancellationToken);

            return operationIds;
        }

        private IQueryable<AsyncTaskOperation> GetPendingQuery(DateTime dateNow)
        {
            return _context.Set<AsyncTaskOperation>()
                            .Where(_ => _.ExecuteAt != null &&
                                        _.ExecuteAt <= dateNow &&
                                        _.Status == AsyncTaskOperationStatus.Pending);
        }

        public async Task<bool> AnyNextAsyncTaskOperationAsync(CancellationToken ct = default)
        {
            var result = await AnyNextAsyncTaskOperationAsyncQuery(_context.As<DbContext>(), ct, DateTime.Now);
            return result;
        }

        private static readonly Func<DbContext, CancellationToken, DateTime, Task<bool>>
            AnyNextAsyncTaskOperationAsyncQuery =
            EF.CompileAsyncQuery((DbContext context, CancellationToken ct, DateTime dateNow) =>
                context.Set<AsyncTaskOperation>()
                    .Where(_ => _.ExecuteAt != null &&
                                _.ExecuteAt <= dateNow &&
                                _.Status == AsyncTaskOperationStatus.Pending)
                    .Any());

        public TAsyncTaskOperationHandler CreateHandler<TAsyncTaskOperationHandler>()
            where TAsyncTaskOperationHandler : IAsyncTaskOperationHandler
        {
            var asyncTaskOperationHandler = _serviceProvider
                .GetService<TAsyncTaskOperationHandler>();

            return asyncTaskOperationHandler;
        }

        public async Task<IEnumerable<AsyncTaskOperation>> EnqueueAsync(IEnumerable<IAsyncTaskOperationHandler> asyncTaskOperationHandlers, TimeSpan? delay = null, int? retryCount = null, AsyncTaskOperationPriority priority = AsyncTaskOperationPriority.Medium)
        {
            var asyncTaskOperationIds = new List<AsyncTaskOperation>();
            var sortIndex = 0;

            foreach (var asyncTaskOperationHandler in asyncTaskOperationHandlers)
            {
                asyncTaskOperationIds.AddRange(
                    await EnqueueAsync(asyncTaskOperationHandler, sortIndex, delay, retryCount: retryCount, priority: priority));

                sortIndex++;
            }

            return asyncTaskOperationIds;
        }

        public async Task<IEnumerable<AsyncTaskOperation>> EnqueueAsync<TAsyncTaskOperationHandler>(int sortIndex = 0, TimeSpan? delay = null, int? retryCount = null, AsyncTaskOperationPriority priority = AsyncTaskOperationPriority.Medium)
            where TAsyncTaskOperationHandler : IAsyncTaskOperationHandler
        {
            var asyncTaskOperationHandler = CreateHandler<TAsyncTaskOperationHandler>();

            return await EnqueueAsync(asyncTaskOperationHandler, sortIndex, delay, retryCount: retryCount, priority: priority);
        }

        private const int _defaultMaxRetryCount = 0;

        public async Task<IEnumerable<AsyncTaskOperation>> EnqueueAsync(IAsyncTaskOperationHandler asyncTaskOperationHandler, int sortIndex = 0, TimeSpan? delay = null, int? retryCount = null, AsyncTaskOperationPriority priority = AsyncTaskOperationPriority.Medium)
        {
            if (await HasAsyncTaskInQueueAsync(asyncTaskOperationHandler))
                return await GetAsyncTaskIdsInQueueAsync(asyncTaskOperationHandler);

            var currentUserId = _serviceProvider.GetService<ICurrentUserService>().GetCurrentUserId();

            var parameter = JsonConvert.SerializeObject(asyncTaskOperationHandler, new JsonSerializerSettings());

            var now = DateTime.Now;

            var asyncTask = new AsyncTaskOperation()
            {
                OperationHandlerId = asyncTaskOperationHandler.GetOperationHandlerId(),
                OperationKey = asyncTaskOperationHandler.OperationKey,
                ReferenceId = asyncTaskOperationHandler.ReferenceId,
                ParameterSerialized = parameter,
                ExecuteById = currentUserId,
                CreatedAt = now,
                ExecuteAt = now.Add(delay ?? TimeSpan.Zero),
                Status = AsyncTaskOperationStatus.Pending,
                Priority = priority,
                SortIndex = sortIndex,
                RetryCount = 0,
                MaxRetryCount = retryCount ?? _defaultMaxRetryCount
            };

            await _context.Set<AsyncTaskOperation>().AddAsync(asyncTask);

            return new[] { asyncTask };
        }

        public async Task HandleTimeout()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var olderThen = DateTime.Now.AddSeconds(-1 * DefaultAsyncTaskTimeOutInSeconds);
                var context = scope.ServiceProvider.GetService<IDbContext>();

                var query = context.Set<AsyncTaskOperation>()
                      .Where(_ => _.Status == AsyncTaskOperationStatus.Executing &&
                                  _.StartedAt < olderThen);

                if (query.Any())
                {
                    await query.ExecuteUpdateAsync(_ =>
                        _.SetProperty(b => b.Status, AsyncTaskOperationStatus.Timeout));
                }
            }
        }

        public async Task Delete(DateTime olderThen)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<IDbContext>();

                var query = context.Set<AsyncTaskOperation>()
                    .Where(_ => _.CreatedAt < olderThen);

                if (query.Any())
                {
                    await query.ExecuteDeleteAsync();
                }
            }
        }

        private async Task<bool> HasAsyncTaskInQueueAsync(IAsyncTaskOperationHandler asyncTaskOperationHandler)
        {
            var hasTaskInQueueLocal = _context.Set<AsyncTaskOperation>()
                .Local
                .Any(o => o.OperationKey == asyncTaskOperationHandler.OperationKey &&
                          o.Status == AsyncTaskOperationStatus.Pending);

            if (hasTaskInQueueLocal)
                return true;

            var hasTaskInQueue = await _context.Set<AsyncTaskOperation>()
                .AnyAsync(o => o.OperationKey == asyncTaskOperationHandler.OperationKey &&
                          o.Status == AsyncTaskOperationStatus.Pending);

            return hasTaskInQueue;
        }

        private async Task<IEnumerable<AsyncTaskOperation>> GetAsyncTaskIdsInQueueAsync(IAsyncTaskOperationHandler asyncTaskOperationHandler)
        {
            var taskInQueueLocal = await _context.Set<AsyncTaskOperation>()
                .Where(o => o.OperationKey == asyncTaskOperationHandler.OperationKey &&
                            o.Status == AsyncTaskOperationStatus.Pending)
                .ToListAsync();

            return taskInQueueLocal;
        }

        private Task<bool> AnyNotFinishedAsyncTasksAsync(params Guid[] referenceIds)
        {
            var timeout = DateTime.Now.AddSeconds(-1 * DefaultAsyncTaskTimeOutInSeconds);

            if (referenceIds.Length == 1)
            {
                var referenceId = referenceIds[0];

                return _context.Set<AsyncTaskOperation>()
                    .Where(o => o.ReferenceId == referenceId &&
                                (o.Status == AsyncTaskOperationStatus.Pending ||
                                 o.Status == AsyncTaskOperationStatus.Executing))
                    .AnyAsync();
            }
            else
            {
                return _context.Set<AsyncTaskOperation>()
                    .Where(o => o.ReferenceId != null &&
                                referenceIds.Contains(o.ReferenceId.Value) &&
                                (o.Status == AsyncTaskOperationStatus.Pending ||
                                 o.Status == AsyncTaskOperationStatus.Executing))
                    .AnyAsync();
            }
        }

        public int DefaultAsyncTaskTimeOutInSeconds { get; set; } = 10;

        public Task WaitUntilReferencedAsyncTasksFinished(params Guid[] referenceIds)
        {
            return WaitUntilReferencedAsyncTasksFinished(referenceIds, null, default);
        }

        public async Task WaitUntilReferencedAsyncTasksFinished(Guid[] referenceIds, int? timeoutInSeconds = null, CancellationToken cancellationToken = default)
        {
            var watch = Stopwatch.StartNew();

            if (!timeoutInSeconds.HasValue)
                timeoutInSeconds = DefaultAsyncTaskTimeOutInSeconds;

            var timeout = timeoutInSeconds * 1000;

            while (watch.ElapsedMilliseconds < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var anyNotFinishedAsyncTasks = await AnyNotFinishedAsyncTasksAsync(referenceIds);

                if (!anyNotFinishedAsyncTasks)
                    return;

                await Task.Delay(TimeSpan.FromMilliseconds(25));
            }

            throw new TimeoutException();
        }
    }
}
