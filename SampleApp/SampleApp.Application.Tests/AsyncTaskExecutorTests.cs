using System.Data;
using System.Diagnostics;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.AsyncTasks;
using SoftwaredeveloperDotAt.Infrastructure.Core.Tests;

namespace SampleApp.Application.Tests;

public class AsyncTaskExecutorTests : BaseTest<DomainStartup>
{
    [Test]
    public async Task Enqueue()
    {
        await _context.Set<AsyncTaskOperation>()
            .ExecuteDeleteAsync();

        var executorTask = StartAsyncTaskExecutorAsync();

        for (var i = 0; i < 1000; i++)
        {
            //if (i <= 1 || i % 10 == 0)
            //{
            //    await EnqueueAsync(new Random().Next(10, 30) * 1000, i, AsyncTaskOperationPriority.Low);
            //}
            //else
            //{
            //await EnqueueAsync(new Random().Next(100, 1000), i, AsyncTaskOperationPriority.Critical);
            await EnqueueAsync(0, i, AsyncTaskOperationPriority.Critical);
            //}

            // await Task.Delay(new Random().Next(1, 1));
        }
        await _context.SaveChangesAsync();

        var watch = Stopwatch.StartNew();

        while (GetPendingOperationCount() > 0)
        {
            await Task.Delay(1000);
        }

        Debug.WriteLine($"----------------------------------");
        Debug.WriteLine($"All Finished: {watch.Elapsed}");
        Debug.WriteLine($"----------------------------------");
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

        Debug.WriteLine($"Start #{OperationNo}#");

        await Task.Delay(TaskDelayInMs);

        if (watch.ElapsedMilliseconds > 10000)
        {
            Debug.WriteLine($"[CRITICAL], End #{OperationNo}#, {watch.ElapsedMilliseconds}ms");
        }
        else
        {
            Debug.WriteLine($"End #{OperationNo}#, {watch.ElapsedMilliseconds}ms");
        }
    }
}