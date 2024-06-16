using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportDefault
{
    public static class ISupportDefaultExtensions
    {
        public static Task<TDto> GetDefaultAsync<TDto, TEntity>(
            this EntityService<TEntity> service)
            where TEntity : Entity, ISupportDefault
            where TDto : Dto, new()
        {
            return service.GetSingleAsync<TDto>(_ => _.WhereIsDefault());
        }

        public static ValueTask<IQueryable<TEntity>> GetQueryWhereIsDefaultAsync<TEntity>(this EntityService<TEntity> service)
            where TEntity : Entity, ISupportDefault
        {
            return service.GetQueryAsync(_ => _.WhereIsDefault());
        }

        public static IQueryable<T> WhereIsDefault<T>(this IQueryable<T> query)
           where T : ISupportDefault
        {
            return query.Where(_ => _.IsDefault == true);
        }
    }
}
