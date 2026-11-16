namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public interface IDistributedLockStore
{
    Task<bool> TryClaimLeaseAsync(
        string lockId,
        string owner,
        DateTime now,
        DateTime expiresAt,
        CancellationToken cancellationToken);

    Task<bool> TryRenewLeaseAsync(
        string lockId,
        string owner,
        DateTime expiresAt,
        CancellationToken cancellationToken);

    Task ReleaseAsync(
        string lockId,
        string owner,
        CancellationToken cancellationToken);
}
