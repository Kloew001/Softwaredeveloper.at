namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;

public class AllAccessCondition<TEntity> : IAccessCondition<TEntity>
    where TEntity : Entity
{
    public ValueTask<bool> CanReadAsync(TEntity entity) => ValueTask.FromResult(true);

    public ValueTask<bool> CanCreateAsync(TEntity entity) => ValueTask.FromResult(true);

    public ValueTask<bool> CanUpdateAsync(TEntity entity) => ValueTask.FromResult(true);

    public ValueTask<bool> CanDeleteAsync(TEntity entity) => ValueTask.FromResult(true);

    public ValueTask<bool> CanSaveAsync(TEntity entity) => ValueTask.FromResult(true);

    public ValueTask<IQueryable<TEntity>> CanReadQueryAsync(IQueryable<TEntity> query) => ValueTask.FromResult(query);
}
