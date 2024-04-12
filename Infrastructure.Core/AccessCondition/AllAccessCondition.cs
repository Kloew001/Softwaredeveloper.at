using SixLabors.Fonts.Tables.TrueType;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public class AllAccessCondition<TEntity> : BaseAccessCondition<TEntity>, IScopedDependency
        where TEntity : Entity
    {
        public override Task<bool> CanReadAsync(TEntity entity) => Task.FromResult(true);

        public override Task<bool> CanCreateAsync(TEntity entity) => Task.FromResult(true);

        public override Task<bool> CanUpdateAsync(TEntity entity) => Task.FromResult(true);

        public override Task<bool> CanDeleteAsync(TEntity entity) => Task.FromResult(true);

        public override Task<bool> CanSaveAsync(TEntity entity) => Task.FromResult(true);

        public override IQueryable<TEntity> CanReadQuery(IQueryable<TEntity> query) => query;
    }
}
