using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

using System.Linq.Expressions;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

public static class EntityExtensions
{
    public static IQueryable<T> GetDbSetByType<T>(this IDbContext context, Type entityType)
    {
        var method = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes);

        // Make the method generic using the provided entity type.
        var genericMethod = method.MakeGenericMethod(entityType);

        // Invoke the method on the context to get the DbSet.
        var dbSet = genericMethod.Invoke(context, null);

        return (dbSet as IQueryable).OfType<T>();
    }

    public static IDbContext ResolveDbContext(this IEntity entity)
    {
        var lazyLoader = (ILazyLoader)entity.GetType()
            .GetProperty("LazyLoader")
            .GetValue(entity, null);

        var dbContext = (IDbContext)lazyLoader.GetType()
            .GetProperty("Context", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(lazyLoader, null);

        return dbContext;
    }

    public static TProperty GetOriginalValue<TEntity, TProperty>(
        this TEntity entity,
        Expression<Func<TEntity, TProperty>> propertyExpression)
        where TEntity : Entity
    {
        var context = entity.ResolveDbContext();
        return GetOriginalValue(context, entity, propertyExpression);
    }

    public static TProperty GetOriginalValue<TEntity, TProperty>(
        this IDbContext context,
        TEntity entity,
        Expression<Func<TEntity, TProperty>> propertyExpression)
        where TEntity : Entity
    {
        var propertyInfo = GetPropertyInfo(context, entity, propertyExpression);

        return propertyInfo.OriginalValue;
    }

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

    public static PropertyEntry<TEntity, TProperty> GetPropertyInfo<TEntity, TProperty>(
        this TEntity entity,
        Expression<Func<TEntity, TProperty>> propertyExpression)
        where TEntity : Entity
    {
        var context = entity.ResolveDbContext();

        return context.GetPropertyInfo(entity, propertyExpression);
    }

    public static PropertyEntry<TEntity, TProperty> GetPropertyInfo<TEntity, TProperty>(
        this IDbContext context,
        TEntity entity,
        Expression<Func<TEntity, TProperty>> propertyExpression)
        where TEntity : Entity
    {
        return context.Entry(entity).Property(propertyExpression);
    }

    public static void Reload<TEntity>(
        this TEntity entity)
        where TEntity : Entity
    {
        var context = entity.ResolveDbContext();
        var entityEntry = context.Entry(entity);

        entityEntry.Reload();

        foreach (var navigation in entityEntry.Navigations)
        {
            if (navigation is CollectionEntry collectionEntry && collectionEntry.IsLoaded)
            {
                collectionEntry.Reload();
            }
            else if (navigation is ReferenceEntry referenceEntry && referenceEntry.IsLoaded)
            {
                referenceEntry.Reload();
            }
        }
    }

    public static void LoadReference<TEntity, TElement>(
        this TEntity entity,
        Expression<Func<TEntity, TElement>> navigationProperty)
        where TElement : class
        where TEntity : Entity
    {
        var context = entity.ResolveDbContext();
        var reference = context.Entry(entity).Reference(navigationProperty);

        if (!reference.IsLoaded)
            reference.Load();
    }

    public static void ReloadReference<TEntity, TElement>(
        this TEntity entity,
        Expression<Func<TEntity, TElement>> navigationProperty)
        where TElement : class
        where TEntity : Entity
    {
        var context = entity.ResolveDbContext();
        var reference = context.Entry(entity).Reference(navigationProperty);

        if (reference.IsLoaded)
            reference.Reload();
        else
            reference.Load();
    }

    public static void LoadCollection<TEntity, TElement>(
        this TEntity entity,
        Expression<Func<TEntity, IEnumerable<TElement>>> navigationProperty)
        where TEntity : Entity
        where TElement : class
    {
        var context = entity.ResolveDbContext();
        var collection = context.Entry(entity).Collection(navigationProperty);

        if (!collection.IsLoaded)
            collection.Load();
    }

    public static void ReloadCollection<TEntity, TElement>(
        this TEntity entity,
        Expression<Func<TEntity, IEnumerable<TElement>>> navigationProperty)
        where TEntity : Entity
        where TElement : class
    {
        var context = entity.ResolveDbContext();
        var collection = context.Entry(entity).Collection(navigationProperty);

        if (collection.IsLoaded)
            collection.Reload();
        else
            collection.Load();
    }

    public static void Reload(this CollectionEntry source)
    {
        if (source.CurrentValue != null)
        {
            foreach (var item in source.CurrentValue)
                source.EntityEntry.Context.Entry(item).State = EntityState.Detached;
            source.CurrentValue = null;
        }
        source.IsLoaded = false;
        source.Load();
    }

    public static void Reload(this ReferenceEntry source)
    {
        if (source.CurrentValue != null)
        {
            var entry = source.EntityEntry.Context.Entry(source.CurrentValue);
            if (entry.State == EntityState.Unchanged)
            {
                entry.Reload();
                return;
            }
            else if (entry.State == EntityState.Detached)
            {
                source.CurrentValue = null;
            }
            else
            {
                //return;
            }
        }
        source.IsLoaded = false;
        source.Load();
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