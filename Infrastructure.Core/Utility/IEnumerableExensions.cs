namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public interface ISupportIndex
    {
        int Index { get; set; }
    }

    public interface ISupportDisplayName
    {
        string DisplayName { get; set; }
    }

    public interface ISupportValidDate
    {
        DateTime? ValidFrom { get; set; }
        DateTime? ValidTo { get; set; }
    }

    public static class IEnumerableUtility
    {
        public static IEnumerable<T> OrderByIndex<T>(this IEnumerable<T> query)
            where T : ISupportIndex
        {
            return query.OrderBy(_ => _.Index);
        }

        public static IEnumerable<T> OrderByDisplayName<T>(this IEnumerable<T> query)
            where T : ISupportDisplayName
        {
            return query.OrderBy(_ => _.DisplayName);
        }

        public static IEnumerable<T> IsValidDateIncluded<T>(this IEnumerable<T> query, DateTime? validDate = null)
            where T : ISupportValidDate
        {
            if (validDate == null)
                validDate = DateTime.Now;

            return query
                .Where(_ => (_.ValidFrom.HasValue == false || _.ValidFrom <= validDate) &&
                            (_.ValidTo.HasValue == false || _.ValidTo >= validDate));
        }

        public static IEnumerable<T> IsValidDateIntersect<T>(this IEnumerable<T> query, ISupportValidDate validDate)
            where T : ISupportValidDate
        {
            return query.IsValidDateIntersect(validDate.ValidFrom, validDate.ValidTo);
        }

        public static IEnumerable<T> IsValidDateIntersect<T>(this IEnumerable<T> query, DateTime? dateFrom = null, DateTime? dateTo = null)
        where T : ISupportValidDate
        {
            if (dateFrom.HasValue)
                query = query
                .Where(_ => _.ValidTo.HasValue == false || _.ValidTo >= dateFrom);

            if (dateTo.HasValue)
                query = query
                .Where(_ => _.ValidFrom.HasValue == false || _.ValidFrom <= dateTo);

            return query;
        }
    }

    public static class IQueryableUtility
    {
        public static IQueryable<T> OrderByIndex<T>(this IQueryable<T> query)
            where T : ISupportIndex
        {
            return query.OrderBy(_ => _.Index);
        }

        public static IQueryable<T> OrderByDisplayName<T>(this IQueryable<T> query)
            where T : ISupportDisplayName
        {
            return query.OrderBy(_ => _.DisplayName);
        }

        public static IQueryable<T> IsValidDateIncluded<T>(this IQueryable<T> query, DateTime? validDate = null)
            where T : ISupportValidDate
        {
            if (validDate == null)
                validDate = DateTime.Now;

            return query
                .Where(_ => (_.ValidFrom.HasValue == false || _.ValidFrom <= validDate) &&
                            (_.ValidTo.HasValue == false || _.ValidTo >= validDate));
        }

        public static IQueryable<T> IsValidDateIntersect<T>(this IQueryable<T> query, ISupportValidDate validDate)
            where T : ISupportValidDate
        {
            return IQueryableUtility.IsValidDateIntersect<T>(query, validDate.ValidFrom, validDate.ValidTo).AsQueryable<T>();
        }

        public static IQueryable<T> IsValidDateIntersect<T>(this IQueryable<T> query, DateTime? dateFrom = null, DateTime? dateTo = null)
            where T : ISupportValidDate
        {
            if(dateFrom.HasValue)
                query = query
                .Where(_ => _.ValidTo.HasValue == false || _.ValidTo >= dateFrom);

            if (dateTo.HasValue)
                query = query
                .Where(_ => _.ValidFrom.HasValue == false || _.ValidFrom <= dateTo);

            return query;
        }
    }
}
