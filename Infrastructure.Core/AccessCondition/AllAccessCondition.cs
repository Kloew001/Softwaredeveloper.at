using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public abstract class AllAccessCondition<TEntity> : BaseAccessCondition<TEntity>, IScopedService
        where TEntity : BaseEntity
    {
        public override bool CanRead(TEntity entity) => true;

        public override bool CanCreate(TEntity entity) => true;

        public override bool CanDelete(TEntity entity) => true;

        public override bool CanUpdate(TEntity entity) => true;

        public override IQueryable<TEntity> CanReadQuery(IQueryable<TEntity> query) => query;
    }
}
