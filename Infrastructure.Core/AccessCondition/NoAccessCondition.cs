using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public abstract class NoAccessCondition<TEntity> : BaseAccessCondition<TEntity>, IScopedService
        where TEntity : BaseEntity
    {
        public override Task<bool> CanReadAsync(TEntity entity) => Task.FromResult(false);

        public override Task<bool> CanCreateAsync(TEntity entity) => Task.FromResult(false);

        public override Task<bool> CanDeleteAsync(TEntity entity) => Task.FromResult(false);

        public override Task<bool> CanUpdateAsync(TEntity entity) => Task.FromResult(false);

        public override IQueryable<TEntity> CanReadQuery(IQueryable<TEntity> query)
        {
            return query.Where(_ => false);
        }
    }
}
