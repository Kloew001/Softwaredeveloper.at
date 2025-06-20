﻿namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public interface IDistributedLock : IDisposable
{
    bool TryExecuteInDistributedLock(string lockId, Func<Task> exclusiveLockTask);
    bool TryAcquireLock(string lockId, int retry = 0);
    Task<bool> TryAcquireLockAsync(string lockId, int retry = 0, CancellationToken cancellationToken = default);
}
