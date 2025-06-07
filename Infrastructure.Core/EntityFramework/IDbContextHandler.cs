using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
public interface IDbContextHandler
{
    Task UpdateDatabaseAsync(DbContext context);

    void DBContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options, string connectionStringKey = "DbContextConnection");

    void OnModelCreating(ModelBuilder modelBuilder);

    void HandleChangeTrackedEntity(DbContext context, DateTime transactionDateTime);
    void HandleEntityAudit(DbContext context, DateTime transactionDateTime);
}
