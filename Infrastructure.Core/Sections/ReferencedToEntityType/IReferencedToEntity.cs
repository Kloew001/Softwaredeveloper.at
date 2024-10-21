using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections;

public interface IReferencedToEntity : IEntity
{
    Guid? ReferenceId { get; set; }
    string ReferenceType { get; set; }
     Entity Reference { get; set; }
}

public static class IReferencedToEntityTypeExtensions
{
    public static void SetReference<TEntitiy>(this IReferencedToEntity entity, TEntitiy referencedEntity)
        where TEntitiy : Entity
    {
        if (referencedEntity == null)
        {
            entity.ReferenceId = null;
            entity.ReferenceType = null;
        }
        else
        {
            entity.ReferenceId = referencedEntity.Id;
            entity.ReferenceType = GetReferenceType(referencedEntity);
        }
    }

    private static string GetReferenceType<TEntitiy>(TEntitiy referencedEntity) 
        where TEntitiy : IEntity
    {
        return referencedEntity.GetType().UnProxy().Name;
    }

    public static IQueryable<T> WhereReferenceId<T>(this IQueryable<T> query, IEntity referencedEntity)
        where T : IReferencedToEntity
    {
        return query
                .Where(_ => _.ReferenceId == referencedEntity.Id && 
                            _.ReferenceType == GetReferenceType(referencedEntity));
    }

    public static async Task<IEntity> GetReferencedEntityAsync(this IReferencedToEntity entity, IDbContext context)
    {
        if( entity.Reference != null )
            return entity.Reference;

        var entityType = AssemblyUtils.AllLoadedTypes()
            .Where(p => p.IsAbstract == false &&
                        p.IsInterface == false)
            .FirstOrDefault(t => t.Name == entity.ReferenceType);

        if (entityType == null)
            throw new InvalidOperationException($"No entity type found with the name {entity.ReferenceType}");

        var dbSet = typeof(DbContext)
            .GetMethod(nameof(DbContext.Set), types: Type.EmptyTypes)
            .MakeGenericMethod(entityType)
            .Invoke(context, null);

        var query = ((IQueryable)dbSet)
            .OfType<IEntity>();

        query = query.Where(_ => _.Id == entity.ReferenceId);

        var referencedEntity = await query.SingleOrDefaultAsync();

        return referencedEntity;
    }
}
