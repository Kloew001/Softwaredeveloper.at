using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices;

public interface IBackgroundTriggerable<T> : IBackgroundTriggerable
    where T : IEntity
{
}

public interface IBackgroundTriggerable
{
}

public interface IBackgroundTrigger
{
    void Trigger();
    Task<bool> WaitAsync(TimeSpan timeout, CancellationToken token);
}

public interface IBackgroundTrigger<T> : IBackgroundTrigger
    where T : IBackgroundTriggerable
{
}

public class BackgroundTrigger<T> : IBackgroundTrigger<T>
    where T : IBackgroundTriggerable
{
    private readonly SemaphoreSlim _signal = new(0, int.MaxValue);

    public void Trigger() => _signal.Release();

    public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken token)
        => _signal.WaitAsync(timeout, token);
}

[ScopedDependency]
public class BackgroundTriggerQueue
{
    private readonly ConcurrentQueue<IBackgroundTrigger> _queue = new();
    private readonly ConcurrentDictionary<Type, byte> _enqueuedTypes = new();
    private readonly IServiceProvider _serviceProvider;

    public BackgroundTriggerQueue(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool Enqueue<T>()
        where T : IBackgroundTrigger
    {
        var trigger = _serviceProvider.GetRequiredService<T>();

        return Enqueue(trigger);
    }

    public bool Enqueue(IBackgroundTrigger trigger)
    {
        if (!_enqueuedTypes.TryAdd(trigger.GetType(), 0))
            return false;

        _queue.Enqueue(trigger);

        return true;
    }

    public bool TryDequeue(out IBackgroundTrigger trigger)
    {
        if (_queue.TryDequeue(out trigger))
        {
            _enqueuedTypes.TryRemove(trigger.GetType(), out _);
            return true;
        }

        return false;
    }

    public IBackgroundTrigger Dequeue()
    {
        if (TryDequeue(out var trigger))
            return trigger;

        return null;
    }
}

public static class BackgroundTriggerableExtensions
{
    public static Type GetTriggerableEntityType(this Type concreteType)
    {
        var iface = concreteType
            .GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IBackgroundTriggerable<>));

        return iface?.GetGenericArguments()[0];
    }
}
