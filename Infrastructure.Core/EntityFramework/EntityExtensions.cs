using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public static class EntityExtensions
    {
        public static bool IsModified<TEntity, TProperty>(
            this TEntity entity,
            Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : Entity
        {
            var context = entity.ResolveDbContext();

            return IsModified(context, entity, propertyExpression);
        }

        public static bool IsModified<TEntity, TProperty>(
            this IDbContext context,
            TEntity entity,
            Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : Entity
        {
            return GetPropertyInfo(context, entity, propertyExpression).IsModified;
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

    //public class ChangeTrackedEntitySaveChangesInterceptor : SaveChangesInterceptor, IScopedService
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