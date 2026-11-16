using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public abstract class EfDistributedLockStoreBase : IDistributedLockStore
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    protected EfDistributedLockStoreBase(IServiceScopeFactory serviceScopeFactory)
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        _serviceScopeFactory = serviceScopeFactory;
    }

    public abstract Task<bool> TryClaimLeaseAsync(
        string lockId,
        string owner,
        DateTime now,
        DateTime expiresAt,
        CancellationToken cancellationToken);

    public async Task<bool> TryRenewLeaseAsync(
        string lockId,
        string owner,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

        var updated = await context.Set<DistributedLock>()
            .Where(_ => _.LockId == lockId && _.Owner == owner)
            .ExecuteUpdateAsync(setters => setters.SetProperty(_ => _.ExpiresAt, expiresAt), cancellationToken);

        return updated > 0;
    }

    public async Task ReleaseAsync(
        string lockId,
        string owner,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

        await context.Set<DistributedLock>()
            .Where(_ => _.LockId == lockId && _.Owner == owner)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
