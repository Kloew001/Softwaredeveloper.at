using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete;

using System.Linq.Expressions;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public static class EntityExtensions
    {
        public static bool IsNotModified<TEntity, TProperty>(
            this TEntity entity,
            Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : Entity
        {
            return !IsModified(entity, propertyExpression);
        }

        public static bool IsModified<TEntity, TProperty>(
        this TEntity entity,
        Expression<Func<TEntity, TProperty>> propertyExpression)
        where TEntity : Entity
        {
            var context = entity.ResolveDbContext();

            return IsModified(context, entity, propertyExpression);
        }

        public static bool IsNotModified<TEntity, TProperty>(
            this IDbContext context,
            TEntity entity,
            Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : Entity
        {
            return !IsModified(context, entity, propertyExpression);
        }

        public static bool IsModified<TEntity, TProperty>(
        this IDbContext context,
        TEntity entity,
        Expression<Func<TEntity, TProperty>> propertyExpression)
        where TEntity : Entity
        {
            return GetPropertyInfo(context, entity, propertyExpression).IsModified;
        }

        public static bool IsNew<TEntity>(
            this TEntity entity)
            where TEntity : Entity
        {
            var context = entity.ResolveDbContext();

            return IsNew(context, entity);
        }

        public static bool IsNew<TEntity>(
            this IDbContext context, TEntity entity)
            where TEntity : Entity
        {
            return context.Entry(entity).State ==
                EntityState.Added;
        }

        public static bool IsModified<TEntity>(
            this TEntity entity)
            where TEntity : Entity
        {
            var context = entity.ResolveDbContext();

            return IsModified(context, entity);
        }

        public static bool IsModified<TEntity>(
            this IDbContext context, TEntity entity)
            where TEntity : Entity
        {
            return context.Entry(entity).State ==
                EntityState.Modified;
        }

        public static bool IsDeleted<TEntity>(
            this TEntity entity)
            where TEntity : Entity
        {
            var context = entity.ResolveDbContext();

            return IsDeleted(context, entity);
        }

        public static bool IsDeleted<TEntity>(
            this IDbContext context, TEntity entity)
            where TEntity : Entity
        {
            if (entity is ISoftDelete softDelete &&
                softDelete.IsDeleted)
                return true;

            return context.Entry(entity).State ==
                EntityState.Deleted;
        }

        public static PropertyEntry GetPropertyInfo<TEntity, TProperty>(
            this TEntity entity,
            Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : Entity
        {
            var context = entity.ResolveDbContext();

            return context.GetPropertyInfo(entity, propertyExpression);
        }

        public static PropertyEntry GetPropertyInfo<TEntity, TProperty>(
            this IDbContext context,
            TEntity entity,
            Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : Entity
        {
            return context.Entry(entity).Property(propertyExpression);
        }

    }

    //public class ChangeTrackedEntitySaveChangesInterceptor : SaveChangesInterceptor, IScopedDependency
    //{
    //    private readonly ICurrentUserService _currentUserService;
    //    public ChangeTrackedEntitySaveChangesInterceptor(ICurrentUserService currentUserService)
    //    {
    //        _currentUserService = currentUserService;
    //    }

    //    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
    //        DbContextEventData eventData,
    //        InterceptionResult<int> result,
    //        CancellationToken cancellationToken = default)
    //    {
    //        if (eventData.Context is not null)
    //        {
    //            UpdateChangeTrackedEntity(eventData.Context);
    //        }

    //        return base.SavingChangesAsync(eventData, result, cancellationToken);
    //    }

    //}
}