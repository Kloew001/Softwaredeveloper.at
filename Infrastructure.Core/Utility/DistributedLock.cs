namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public interface IDistributedLock : IAsyncDisposable
{
    Task<bool> TryAcquireLockAsync(string lockId, int retry = 3, CancellationToken cancellationToken = default);
}