using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AsyncTasks;

public enum AsyncTaskOperationPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum AsyncTaskOperationStatus
{
    Pending = 0,
    Executing = 1,
    Success = 2,
    Failed = 3,
    Timeout = 4,
    Duplicate = 5,
}

[AttributeUsage(AttributeTargets.Class)]
public class OperationHandlerAttribute :
    Attribute
{
    public Guid Id { get; set; }

    public OperationHandlerAttribute(string guidId)
    {
        this.Id = Guid.Parse(guidId);
    }

    public OperationHandlerAttribute(Guid id)
    {
        this.Id = id;
    }
}

[TransientDependency]
public interface IAsyncTaskOperationHandler
{
    public string OperationKey { get; }
    public Guid? ReferenceId { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}

public static class IAsyncTaskOperationHandlerExtensions
{
    public static Guid GetOperationHandlerId(this IAsyncTaskOperationHandler asyncTaskOperationHandler)
    {

        return asyncTaskOperationHandler.GetType().GetCustomAttribute<OperationHandlerAttribute>().Id;
    }
}