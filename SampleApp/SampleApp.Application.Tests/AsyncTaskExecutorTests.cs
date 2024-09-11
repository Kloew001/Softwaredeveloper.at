using Infrastructure.Core.Tests;

using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core;
using SoftwaredeveloperDotAt.Infrastructure.Core.AsyncTasks;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

using System.Data;
using System.Diagnostics;

namespace SampleApp.Application.Tests
{
    public class AsyncTaskExecutorTests : BaseTest<DomainStartup>
    {
        [Test]
        public async Task Enqueue()
        {
            var pendingOperationCount = GetPendingOperationCount();

            Assert.That(pendingOperationCount == 0);

            var executorTask = StartAsyncTaskExecutorAsync();

            for (int i = 0; i < 100; i++)
            {
                if (i % 5 == 0)
                {
                    await EnqueueAsync(10 * 1000, i, AsyncTaskOperationPriority.Low);
                }
                else
                {
                    await EnqueueAsync(new Random().Next(1, 800), i, AsyncTaskOperationPriority.Critical);
                }
                await _context.SaveChangesAsync();

                await Task.Delay(new Random().Next(1, 1000));
            }

            while (GetPendingOperationCount() > 0)
            {
                await Task.Delay(1000);
            }

            Debug.WriteLine($"----------------------------------");
            Debug.WriteLine($"All Finished");
            Debug.WriteLine($"----------------------------------");
        }

        private Task StartAsyncTaskExecutorAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                using (var childScope = _serviceScope.ServiceProvider.CreateChildScope())
                {
                    var asyncTaskExecutorHostedService =
                        childScope.ServiceProvider
                        .GetService<AsyncTaskExecutorHostedService>();

                    var applicationSettings = childScope.ServiceProvider
                        .GetService<IApplicationSettings>();

                    applicationSettings.HostedServices.TryGetValue(typeof(AsyncTaskExecutorHostedService).Name, out HostedServicesConfiguration hostedServicesConfiguration);

                    hostedServicesConfiguration.BatchSize = 10;
                    hostedServicesConfiguration.Interval = TimeSpan.FromSeconds(1);

                    await asyncTaskExecutorHostedService.ExecuteAsync(cancellationToken);
                }
            }, cancellationToken);
        }


        private int GetPendingOperationCount()
        {
            var dateNow = DateTime.Now;

            return _context.Set<AsyncTaskOperation>()
                        .Where(_ => _.ExecuteAt != null &&
                                    _.ExecuteAt <= dateNow &&
                                    (_.Status == AsyncTaskOperationStatus.Pending ||
                                     _.Status == AsyncTaskOperationStatus.Executing))
                        .Count();
        }

        private async Task EnqueueAsync(int taskDelayInMs, int operationNo, AsyncTaskOperationPriority priority)
        {
            var asyncTaskExecutor =
                _serviceScope.ServiceProvider
                .GetService<AsyncTaskExecutor>();

            var operation = asyncTaskExecutor.CreateHandler<FakeRunningAsyncTaskOperationHandler>();
            operation.ReferenceId = Guid.NewGuid();
            operation.TaskDelayInMs = taskDelayInMs;
            operation.OperationNo = operationNo;

            await asyncTaskExecutor.EnqueueAsync(operation, priority: priority);
        }
    }

    [OperationHandler(OperationHandlerId)]
    public class FakeRunningAsyncTaskOperationHandler : IAsyncTaskOperationHandler
    {
        public const string OperationHandlerId = "E6B33904-C873-4EB9-B78B-0EE7B7EACF63";
        public string OperationKey
            => $"{nameof(FakeRunningAsyncTaskOperationHandler)}_{ReferenceId}";

        public Guid? ReferenceId { get; set; }

        public int OperationNo { get; set; }

        public int TaskDelayInMs { get; set; }

        public async Task ExecuteAsync(CancellationToken ct)
        {
            var watch = Stopwatch.StartNew();

            Debug.WriteLine($"Start {OperationNo}, {DateTime.Now:HH:mm:ss.ffff}");

            await Task.Delay(TaskDelayInMs);

            if (watch.ElapsedMilliseconds > 10000)
            {
                Debug.WriteLine($"[CRITICAL], End {OperationNo}, {watch.ElapsedMilliseconds}ms, {DateTime.Now:HH:mm:ss.ffff}");
            }
            else
            {
                Debug.WriteLine($"End {OperationNo}, {watch.ElapsedMilliseconds}ms, {DateTime.Now:HH:mm:ss.ffff}");
            }
        }
    }
}