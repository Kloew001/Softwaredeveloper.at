namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public abstract class NoAccessCondition<TEntity> : IAccessCondition<TEntity>, IScopedDependency
        where TEntity : Entity
    {
        public Task<bool> CanReadAsync(TEntity entity) => Task.FromResult(false);

        public Task<bool> CanCreateAsync(TEntity entity) => Task.FromResult(false);

        public Task<bool> CanDeleteAsync(TEntity entity) => Task.FromResult(false);

        public Task<bool> CanUpdateAsync(TEntity entity) => Task.FromResult(false);

        public Task<bool> CanSaveAsync(TEntity entity) => Task.FromResult(false);

        public Task<IQueryable<TEntity>> CanReadQuery(IQueryable<TEntity> query)
        {
            return Task.FromResult(query.Where(_ => false));
        }
    }
}
