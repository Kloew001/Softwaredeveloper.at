namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public class AllAccessCondition<TEntity> : IAccessCondition<TEntity>, IScopedDependency
        where TEntity : Entity
    {
        public Task<bool> CanReadAsync(TEntity entity) => Task.FromResult(true);

        public Task<bool> CanCreateAsync(TEntity entity) => Task.FromResult(true);

        public Task<bool> CanUpdateAsync(TEntity entity) => Task.FromResult(true);

        public Task<bool> CanDeleteAsync(TEntity entity) => Task.FromResult(true);

        public Task<bool> CanSaveAsync(TEntity entity) => Task.FromResult(true);

        public Task<IQueryable<TEntity>> CanReadQuery(IQueryable<TEntity> query) => Task.FromResult(query);
    }
}
