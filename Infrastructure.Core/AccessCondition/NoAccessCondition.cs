namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;

public abstract class NoAccessCondition<TEntity> : IAccessCondition<TEntity>
    where TEntity : Entity
{
    public ValueTask<bool> CanReadAsync(TEntity entity) => ValueTask.FromResult(false);

    public ValueTask<bool> CanCreateAsync(TEntity entity) => ValueTask.FromResult(false);

    public ValueTask<bool> CanDeleteAsync(TEntity entity) => ValueTask.FromResult(false);

    public ValueTask<bool> CanUpdateAsync(TEntity entity) => ValueTask.FromResult(false);

    public ValueTask<bool> CanSaveAsync(TEntity entity) => ValueTask.FromResult(false);

    public ValueTask<IQueryable<TEntity>> CanReadQueryAsync(IQueryable<TEntity> query)
    {
        return ValueTask.FromResult(query.Where(_ => false));
    }
}