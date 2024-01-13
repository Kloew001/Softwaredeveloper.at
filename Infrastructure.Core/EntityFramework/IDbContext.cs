using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public interface IDbContext
    {
        DatabaseFacade Database { get; }

        DbSet<TEntity> Set<TEntity>()
            where TEntity : class;

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
            where TEntity : class;

        TEntity CreateProxy<TEntity>(params object[] constructorArguments)
                where TEntity : class;

        EntityEntry<TEntity> Add<TEntity>(TEntity entity)
            where TEntity : class;

        ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
                where TEntity : class;
        void AddRange(IEnumerable<object> entities);
        Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);

        EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
            where TEntity : class;

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    //public class ChangeTrackedEntitySaveChangesInterceptor : SaveChangesInterceptor, IScopedService
    //{
    //    private readonly ICurrentUserService _currentUserService;
    //    public ChangeTrackedEntitySaveChangesInterceptor(ICurrentUserService currentUserService)
    //    {
    //        _currentUserService = currentUserService;
    //    }

    //    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
    //        DbContextEventData eventData,
    //        InterceptionResult<int> result,
    //        CancellationToken cancellationToken = default)
    //    {
    //        if (eventData.Context is not null)
    //        {
    //            UpdateChangeTrackedEntity(eventData.Context);
    //        }

    //        return base.SavingChangesAsync(eventData, result, cancellationToken);
    //    }

    //}
}