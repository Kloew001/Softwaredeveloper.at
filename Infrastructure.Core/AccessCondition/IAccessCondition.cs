namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;

[ScopedDependency<IAccessCondition>]
public interface IAccessCondition
{
    ValueTask<bool> CanReadAsync(IEntity entity);
    ValueTask<IQueryable<IEntity>> CanReadQueryAsync(IQueryable<IEntity> query);

    ValueTask<bool> CanCreateAsync(IEntity entity);
    ValueTask<bool> CanUpdateAsync(IEntity entity);
    ValueTask<bool> CanDeleteAsync(IEntity entity);
    ValueTask<bool> CanSaveAsync(IEntity entity);
}

public interface IAccessCondition<TEntity> : IAccessCondition, ITypedScopedDependency<IAccessCondition<TEntity>>
    where TEntity : IEntity
{
    ValueTask<bool> CanReadAsync(TEntity entity);
    ValueTask<IQueryable<TEntity>> CanReadQueryAsync(IQueryable<TEntity> query);

    ValueTask<bool> CanCreateAsync(TEntity entity);
    ValueTask<bool> CanUpdateAsync(TEntity entity);
    ValueTask<bool> CanDeleteAsync(TEntity entity);
    ValueTask<bool> CanSaveAsync(TEntity entity);

    ValueTask<bool> IAccessCondition.CanReadAsync(IEntity entity) => CanReadAsync((TEntity)entity);
    ValueTask<IQueryable<IEntity>> IAccessCondition.CanReadQueryAsync(IQueryable<IEntity> query) => CanReadQueryAsync(query);
    ValueTask<bool> IAccessCondition.CanCreateAsync(IEntity entity) => CanCreateAsync((TEntity)entity);
    ValueTask<bool> IAccessCondition.CanUpdateAsync(IEntity entity) => CanUpdateAsync((TEntity)entity);
    ValueTask<bool> IAccessCondition.CanDeleteAsync(IEntity entity) => CanDeleteAsync((TEntity)entity);
    ValueTask<bool> IAccessCondition.CanSaveAsync(IEntity entity) => CanSaveAsync((TEntity)entity);
}