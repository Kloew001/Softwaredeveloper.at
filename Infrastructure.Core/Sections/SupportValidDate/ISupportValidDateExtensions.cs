using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;
using FluentValidation;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportValidDate
{
    public static class ISupportValidDateExtensions
    {
        public static IEnumerable<T> IsValidDateIncluded<T>(this IEnumerable<T> query, DateTime? validDate = null)
            where T : ISupportValidDateRange
        {
            if (validDate == null)
                validDate = DateTime.Now;

            return query
                .Where(_ => (_.ValidFrom.HasValue == false || _.ValidFrom <= validDate) &&
                            (_.ValidTo.HasValue == false || _.ValidTo >= validDate));
        }

        public static IEnumerable<T> IsValidDateIntersect<T>(this IEnumerable<T> query, ISupportValidDateRange validDate)
            where T : ISupportValidDateRange
        {
            return query.IsValidDateIntersect(validDate.ValidFrom, validDate.ValidTo);
        }

        public static IEnumerable<T> IsValidDateIntersect<T>(this IEnumerable<T> query, DateTime? dateFrom = null, DateTime? dateTo = null)
        where T : ISupportValidDateRange
        {
            if (dateFrom.HasValue)
                query = query
                .Where(_ => _.ValidTo.HasValue == false || _.ValidTo >= dateFrom);

            if (dateTo.HasValue)
                query = query
                .Where(_ => _.ValidFrom.HasValue == false || _.ValidFrom <= dateTo);

            return query;
        }

        public static IQueryable<T> IsValidDateIncluded<T>(this IQueryable<T> query, DateTime? validDate = null)
           where T : ISupportValidDateRange
        {
            if (validDate == null)
                validDate = DateTime.Now;

            return query
                .Where(_ => (_.ValidFrom.HasValue == false || _.ValidFrom <= validDate) &&
                            (_.ValidTo.HasValue == false || _.ValidTo >= validDate));
        }

        public static IQueryable<T> IsValidDateIntersect<T>(this IQueryable<T> query, ISupportValidDateRange validDate)
            where T : ISupportValidDateRange
        {
            return query.IsValidDateIntersect(validDate.ValidFrom, validDate.ValidTo).AsQueryable();
        }

        public static IQueryable<T> IsValidDateIntersect<T>(this IQueryable<T> query, DateTime? dateFrom = null, DateTime? dateTo = null)
            where T : ISupportValidDateRange
        {
            if (dateFrom.HasValue)
                query = query
                .Where(_ => _.ValidTo.HasValue == false || _.ValidTo >= dateFrom);

            if (dateTo.HasValue)
                query = query
                .Where(_ => _.ValidFrom.HasValue == false || _.ValidFrom <= dateTo);

            return query;
        }

        public static Task<IEnumerable<TDto>> GetAllValidAsync<TDto, TEntity>(
            this EntityService<TEntity> service,
            DateTime? validDate = null)
            where TEntity : Entity, ISupportValidDateRange
            where TDto : Dto, new()
        {
            if (validDate == null)
                validDate = DateTime.Now;

            return service.GetCollectionAsync<TDto>(_ =>
                    _.IsValidDateIncluded(validDate));
        }

        public static Task<IQueryable<TEntity>> GetAllValid<TEntity>(this EntityService<TEntity> service, DateTime? validDate = null)
            where TEntity : Entity, ISupportValidDateRange
        {
            if (validDate == null)
                validDate = DateTime.Now;

            return service.GetCollectionQueryInternal(
                _ => _.IsValidDateIncluded(validDate));
        }

    }
}
