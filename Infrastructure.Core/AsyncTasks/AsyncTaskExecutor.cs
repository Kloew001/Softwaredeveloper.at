using System.Data;
using System.Diagnostics;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                asyncTaskExecutor.ExecuteBatchAsync(_hostedServicesConfiguration.BatchSize, cancellationToken),
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

        public async Task<bool> ExecuteAsync(Guid operationId, CancellationToken cancellationToken)
        {
            return await ExecuteAsyncTaskOperationIdAsync(operationId, cancellationToken);
        }

        public async Task ExecuteBatchAsync(int batchSize = 10, CancellationToken cancellationToken = default)
        {
            IEnumerable<Guid> asyncTaskOperationIds;

            using (var distributedLock = _serviceProvider.GetRequiredService<IDistributedLock>())
            {
                var lockName = $"{nameof(AsyncTaskExecutor)}_{nameof(GetNextAsyncTaskOperationIdsAsync)}";
                if (distributedLock.TryAcquireLock(lockName, 3) == false)
                    throw new InvalidOperationException();

                asyncTaskOperationIds = await GetNextAsyncTaskOperationIdsAsync(batchSize, cancellationToken);

                await SetExecutingAsync(asyncTaskOperationIds);
            }

            var tasks = new List<Task>();
            var scopes = new List<IServiceScope>();
            foreach (var asyncTaskOperationId in asyncTaskOperationIds)
            {
                var scope = _serviceProvider.CreateScope();
                var asyncTaskExecutor = scope.ServiceProvider.GetService<AsyncTaskExecutor>();
                var task = asyncTaskExecutor.ExecuteAsyncTaskOperationIdAsync(asyncTaskOperationId, cancellationToken);
                tasks.Add(task);
                scopes.Add(scope);
            }

            Task.WaitAll(tasks.ToArray(), cancellationToken);
            scopes.ForEach(_ => _.Dispose());
        }

        private async Task<bool> ExecuteAsyncTaskOperationIdAsync(Guid asyncTaskOperationId, CancellationToken cancellationToken)
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

                    if (asyncTaskOperation.Status == AsyncTaskOperationStatus.Success ||
                        asyncTaskOperation.Status == AsyncTaskOperationStatus.Failed)
                        throw new NotSupportedException();

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

        private Task SetExecutingAsync(Guid asyncTaskOperationId)
        {
            return SetExecutingAsync(new[] { asyncTaskOperationId });
        }

        private async Task SetExecutingAsync(IEnumerable<Guid> asyncTaskOperationIds)
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

        private const int _maxRetryCount = 3;
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

                    if (asyncTaskOperation.RetryCount <= _maxRetryCount)
                    {
                        asyncTaskOperation.Status = AsyncTaskOperationStatus.Pending;
                        asyncTaskOperation.StartedAt = null;
                        asyncTaskOperation.ExecuteAt = DateTime.Now.Add(TimeSpan.FromSeconds(asyncTaskOperation.RetryCount * 5));
                        asyncTaskOperation.RetryCount = asyncTaskOperation.RetryCount + 1;
                        asyncTaskOperation.ErrorMessage = ex.Message;
                    }
                    else
                    {
                        asyncTaskOperation.ExecuteAt = null;
                        asyncTaskOperation.Status = AsyncTaskOperationStatus.Failed;
                        asyncTaskOperation.ErrorMessage = ex.Message;
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex2)
            {
                _logger.LogError(ex2, "Fehler HandleErrorAsync: " + asyncTaskOperationId);
            }
        }

        //private const int _waitingExecuteToRetryInSec = 120;
        private async Task<IEnumerable<Guid>> GetNextAsyncTaskOperationIdsAsync(int take = 1, CancellationToken ct = default)
        {
            var dateNow = DateTime.Now;

            var watch = Stopwatch.StartNew();

            var operationIds = await
                _context.Set<AsyncTaskOperation>()
                .Where(_ => _.ExecuteAt != null &&
                            _.ExecuteAt <= dateNow &&
                            _.Status == AsyncTaskOperationStatus.Pending)
                .OrderBy(_ => _.ExecuteAt)
                .ThenBy(_ => _.SortIndex)
                .Select(_ => _.Id)
                .Take(take)
                .ToListAsync(ct);

            var time = watch.ElapsedMilliseconds;

            return operationIds;
        }

        public async Task<bool> AnyNextAsyncTaskOperationAsync(CancellationToken ct = default)
        {
            var watch = Stopwatch.StartNew();
            var result = await AnyNextAsyncTaskOperationAsyncQuery(_context.As<DbContext>(), ct, DateTime.Now);
            var time = watch.ElapsedMilliseconds;
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

        public async Task<IEnumerable<AsyncTaskOperation>> EnqueueAsync(IEnumerable<IAsyncTaskOperationHandler> asyncTaskOperationHandlers, TimeSpan? delay = null)
        {
            var asyncTaskOperationIds = new List<AsyncTaskOperation>();
            var sortIndex = 0;

            foreach (var asyncTaskOperationHandler in asyncTaskOperationHandlers)
            {
                asyncTaskOperationIds.AddRange(
                    await EnqueueAsync(asyncTaskOperationHandler, sortIndex, delay));

                sortIndex++;
            }

            return asyncTaskOperationIds;
        }

        public async Task<IEnumerable<AsyncTaskOperation>> EnqueueAsync<TAsyncTaskOperationHandler>(int sortIndex = 0, TimeSpan? delay = null)
            where TAsyncTaskOperationHandler : IAsyncTaskOperationHandler
        {
            var asyncTaskOperationHandler = CreateHandler<TAsyncTaskOperationHandler>();

            return await EnqueueAsync(asyncTaskOperationHandler, sortIndex, delay);
        }

        public async Task<IEnumerable<AsyncTaskOperation>> EnqueueAsync(IAsyncTaskOperationHandler asyncTaskOperationHandler, int sortIndex = 0, TimeSpan? delay = null)
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
                SortIndex = sortIndex,
                RetryCount = 0,
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

                await context.Set<AsyncTaskOperation>()
                      .Where(_ => _.Status == AsyncTaskOperationStatus.Executing &&
                                  _.StartedAt < olderThen)
                  .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.Status, AsyncTaskOperationStatus.Timeout));
            }
        }

        public async Task Delete(DateTime olderThen)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<IDbContext>();

                await context.Set<AsyncTaskOperation>()
                    .Where(_ => _.CreatedAt < olderThen)
                    .ExecuteDeleteAsync();
            }
        }

        private async Task<bool> HasAsyncTaskInQueueAsync(IAsyncTaskOperationHandler asyncTaskOperationHandler)
        {
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
                return _context.Set<AsyncTaskOperation>()
                    .Where(o => o.ReferenceId == referenceIds[0] &&
                                o.Status == AsyncTaskOperationStatus.Pending ||
                                o.Status == AsyncTaskOperationStatus.Executing)
                    .AnyAsync();
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
            return WaitUntilReferencedAsyncTasksFinished(null, referenceIds);
        }

        public async Task WaitUntilReferencedAsyncTasksFinished(int? timeoutInSeconds = null, params Guid[] referenceIds)
        {
            var watch = Stopwatch.StartNew();

            if (!timeoutInSeconds.HasValue)
                timeoutInSeconds = DefaultAsyncTaskTimeOutInSeconds;

            var timeout = timeoutInSeconds * 1000;

            while (watch.ElapsedMilliseconds < timeout)
            {
                var watch2 = Stopwatch.StartNew();

                var anyNotFinishedAsyncTasks = await AnyNotFinishedAsyncTasksAsync(referenceIds);
                var howon = watch2.ElapsedMilliseconds;

                if (!anyNotFinishedAsyncTasks)
                    return;

                await Task.Delay(TimeSpan.FromMilliseconds(25));
            }

            throw new TimeoutException();
        }
    }
}
