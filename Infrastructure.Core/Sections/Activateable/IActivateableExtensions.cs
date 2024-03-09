using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Activateable
{
    public static class IActivateableExtensions
    {
        public static Task<IEnumerable<TDto>> GetAllActiveAsync<TDto, TEntity>(
            this EntityService<TEntity> service, bool isEnabled = true)
            where TEntity : Entity, IActivateable
            where TDto : Dto, new()
        {
            return service.GetCollectionAsync<TDto>(_ =>
                    _.IsActive(isEnabled));
        }

        public static Task<IQueryable<TEntity>> GetAllActive<TEntity>(this EntityService<TEntity> service, bool isEnabled = true)
            where TEntity : Entity, IActivateable
        {
            return service.GetCollectionQueryInternal(
                _ => _.IsActive(isEnabled));
        }

        public static IQueryable<T> IsActive<T>(this IQueryable<T> query, bool isEnabled = true)
           where T : IActivateable
        {
            return query.Where(_ => _.IsActive == isEnabled);
        }
    }
}
