using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public abstract class NoAccessCondition<TEntity> : BaseAccessCondition<TEntity>, IScopedService
        where TEntity : BaseEntity
    {
        public override bool CanRead(TEntity entity) => false;

        public override bool CanCreate(TEntity entity) => false;

        public override bool CanDelete(TEntity entity) => false;

        public override bool CanUpdate(TEntity entity) => false;

        public override IQueryable<TEntity> CanReadQuery(IQueryable<TEntity> query)
        {
            return query.Where(_ => false);
        }
    }
}
