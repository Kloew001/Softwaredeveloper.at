using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public interface IAccessCondition
    {
        bool CanRead(BaseEntity entity);
        IQueryable<BaseEntity> CanReadQuery(IQueryable<BaseEntity> query);

        bool CanCreate(BaseEntity entity);
        bool CanUpdate(BaseEntity entity);
        bool CanDelete(BaseEntity entity);
    }

    public interface IAccessCondition<TEntity> : IAccessCondition, ITypedScopedService<IAccessCondition<TEntity>>
        where TEntity : BaseEntity
    {
        bool CanRead(TEntity entity);
        IQueryable<TEntity> CanReadQuery(IQueryable<TEntity> query);

        bool CanCreate(TEntity entity);
        bool CanUpdate(TEntity entity);
        bool CanDelete(TEntity entity);
    }

    public abstract class BaseAccessCondition<TEntity> : IAccessCondition<TEntity>
        where TEntity : BaseEntity
    {
        public abstract bool CanCreate(TEntity entity);

        public abstract bool CanUpdate(TEntity entity);

        public abstract bool CanDelete(TEntity entity);

        public abstract bool CanRead(TEntity entity);

        public abstract IQueryable<TEntity> CanReadQuery(IQueryable<TEntity> query);

        public bool CanCreate(BaseEntity entity) => CanCreate((TEntity)entity);

        public bool CanDelete(BaseEntity entity) => CanDelete((TEntity)entity);

        public bool CanRead(BaseEntity entity) => CanRead((TEntity)entity);

        public IQueryable<BaseEntity> CanReadQuery(IQueryable<BaseEntity> query) => CanReadQuery((IQueryable<TEntity>)query);

        public bool CanUpdate(BaseEntity entity) => CanUpdate((TEntity)entity);
    }
}
