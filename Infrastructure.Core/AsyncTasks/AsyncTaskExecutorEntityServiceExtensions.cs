using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.BinaryContentSection;

public static class AsyncTaskExecutorEntityServiceExtensions
{
    public static TAsyncTaskOperationHandler CreateAsyncTaskHandler<TAsyncTaskOperationHandler, TEntity>(
        this EntityService<TEntity> service)
        where TAsyncTaskOperationHandler : IAsyncTaskOperationHandler

        where TEntity : Entity
    {
        var serviceProvider = service.EntityServiceDependency.ServiceProvider;

        var asyncTaskExecutor = serviceProvider.GetService<AsyncTaskExecutor>();

        var operation = asyncTaskExecutor.CreateHandler<TAsyncTaskOperationHandler>();

        return operation;
    }

    public static async Task<IEnumerable<AsyncTaskOperation>> EnqueueAsyncTaskOperation<TEntity>(
        this EntityService<TEntity> service, IAsyncTaskOperationHandler asyncTaskOperation)

        where TEntity : Entity
    {
        var serviceProvider = service.EntityServiceDependency.ServiceProvider;

        var asyncTaskExecutor = serviceProvider.GetService<AsyncTaskExecutor>();

        return await asyncTaskExecutor.EnqueueAsync(asyncTaskOperation);
    }
}
