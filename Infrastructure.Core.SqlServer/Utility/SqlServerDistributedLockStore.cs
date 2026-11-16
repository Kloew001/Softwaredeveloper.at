using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.SqlServer.Utility;

public class SqlServerDistributedLockStore : EfDistributedLockStoreBase
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SqlServerDistributedLockStore(IServiceScopeFactory serviceScopeFactory)
        : base(serviceScopeFactory)
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override async Task<bool> TryClaimLeaseAsync(
        string lockId,
        string owner,
        DateTime now,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

        var lockIdParameter = new SqlParameter("@lockId", lockId);
        var ownerParameter = new SqlParameter("@owner", owner);
        var expiresAtParameter = new SqlParameter("@expiresAt", expiresAt);
        var nowParameter = new SqlParameter("@now", now);

        var affectedRows = await context.Database.ExecuteSqlRawAsync(
            @"
            UPDATE [lock]
            SET [Owner] = @owner,
                [ExpiresAt] = @expiresAt
            FROM [core].[DistributedLock] AS [lock] WITH (UPDLOCK, HOLDLOCK)
            WHERE [lock].[LockId] = @lockId
              AND ([lock].[ExpiresAt] < @now OR [lock].[Owner] = @owner);

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO [core].[DistributedLock] ([LockId], [Owner], [ExpiresAt])
                SELECT @lockId, @owner, @expiresAt
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM [core].[DistributedLock] WITH (UPDLOCK, HOLDLOCK)
                    WHERE [LockId] = @lockId
                );
            END;",
            [lockIdParameter, ownerParameter, expiresAtParameter, nowParameter],
            cancellationToken);

        return affectedRows > 0;
    }
}
