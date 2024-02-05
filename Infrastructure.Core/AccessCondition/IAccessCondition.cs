using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using System.Linq;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition
{
    public interface IAccessCondition
    {
        Task<bool> CanReadAsync(IEntity entity);
        IQueryable<IEntity> CanReadQuery(IQueryable<IEntity> query);

        Task<bool> CanCreateAsync(IEntity entity);
        Task<bool> CanUpdateAsync(IEntity entity);
        Task<bool> CanDeleteAsync(IEntity entity);
        Task<bool> CanSaveAsync(IEntity entity);
    }

    public interface IAccessCondition<TEntity> : IAccessCondition, ITypedScopedService<IAccessCondition<TEntity>>
        //where TEntity : BaseEntity
    {
        Task<bool> CanReadAsync(TEntity entity);
        IQueryable<TEntity> CanReadQuery(IQueryable<TEntity> query);

        Task<bool> CanCreateAsync(TEntity entity);
        Task<bool> CanUpdateAsync(TEntity entity);
        Task<bool> CanDeleteAsync(TEntity entity);
        Task<bool> CanSaveAsync(TEntity entity);
    }

    public abstract class BaseAccessCondition<TEntity> : IAccessCondition<TEntity>
        where TEntity : IEntity
    {
        public abstract Task<bool> CanCreateAsync(TEntity entity);

        public abstract Task<bool> CanUpdateAsync(TEntity entity);

        public abstract Task<bool> CanDeleteAsync(TEntity entity);

        public abstract Task<bool> CanReadAsync(TEntity entity);

        public abstract Task<bool> CanSaveAsync(TEntity entity);

        public abstract IQueryable<TEntity> CanReadQuery(IQueryable<TEntity> query);

        public Task<bool> CanCreateAsync(IEntity entity) => CanCreateAsync((TEntity)entity);

        public Task<bool> CanDeleteAsync(IEntity entity) => CanDeleteAsync((TEntity)entity);

        public Task<bool> CanReadAsync(IEntity entity) => CanReadAsync((TEntity)entity);

        public IQueryable<IEntity> CanReadQuery (IQueryable<IEntity> query) => CanReadQuery(query);

        public Task<bool> CanUpdateAsync(IEntity entity) => CanUpdateAsync((TEntity)entity);

        public Task<bool> CanSaveAsync(IEntity entity) => CanSaveAsync((TEntity)entity);
    }
}
