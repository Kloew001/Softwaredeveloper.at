using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;
using NpgsqlTypes;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.PostgreSQL.Utility;

public class PostgreSqlDistributedLockStore : EfDistributedLockStoreBase
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PostgreSqlDistributedLockStore(IServiceScopeFactory serviceScopeFactory)
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

        var nowUnspecified = DateTime.SpecifyKind(now, DateTimeKind.Unspecified);
        var expiresAtUnspecified = DateTime.SpecifyKind(expiresAt, DateTimeKind.Unspecified);

        var lockIdParameter = new NpgsqlParameter("lockId", lockId);
        var ownerParameter = new NpgsqlParameter("owner", owner);
        var expiresAtParameter = new NpgsqlParameter("expiresAt", NpgsqlDbType.Timestamp)
        {
            Value = expiresAtUnspecified
        };
        var nowParameter = new NpgsqlParameter("now", NpgsqlDbType.Timestamp)
        {
            Value = nowUnspecified
        };

        var affectedRows = await context.Database.ExecuteSqlRawAsync(
            @"
            INSERT INTO core.""DistributedLock"" (""LockId"", ""Owner"", ""ExpiresAt"")
            VALUES (@lockId, @owner, @expiresAt)
            ON CONFLICT (""LockId"") DO UPDATE
            SET ""Owner"" = EXCLUDED.""Owner"", ""ExpiresAt"" = EXCLUDED.""ExpiresAt""
            WHERE core.""DistributedLock"".""ExpiresAt"" < @now
               OR core.""DistributedLock"".""Owner"" = @owner;",
            [lockIdParameter, ownerParameter, expiresAtParameter, nowParameter],
            cancellationToken);

        return affectedRows > 0;
    }
}
