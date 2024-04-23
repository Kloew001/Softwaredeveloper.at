namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections
{
    public interface IReferencedToEntityType
    {
        Guid? ReferenceId { get; set; }
        string ReferenceType { get; set; }
    }

    public static class IReferencedToEntityTypeExtensions
    {
        public static void SetReference<TEntitiy>(this IReferencedToEntityType entity, TEntitiy reference)
            where TEntitiy : Entity
        {
            entity.ReferenceId = reference.Id;
            entity.ReferenceType = reference.GetType().UnProxy().Name;
        }

        public static IQueryable<T> WhereReferenceId<T>(this IQueryable<T> query, Guid referenceId, string referenceType = null)
            where T : IReferencedToEntityType
        {
            if(referenceType.IsNullOrEmpty())
                return query.Where(_ => _.ReferenceId == referenceId);

            return query
                .Where(_ => _.ReferenceId == referenceId && _.ReferenceType == referenceType);
        }
    }

}
