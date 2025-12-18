using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

public interface IDbContextHandler
{
    Task UpdateDatabaseAsync(DbContext context);

    void DBContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options, string connectionStringKey = "DbContextConnection");

    void OnModelCreating(ModelBuilder modelBuilder);

    void HandleChangeTrackedEntity(DbContext context);
    void HandleEntityAudit(DbContext context);
    void EnqueueBackgroundTrigger(DbContext context);
    void TriggerBackground(DbContext context);
}