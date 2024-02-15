using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual;

using System.Linq.Expressions;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public interface ISupportIndex
    {
        int Index { get; set; }
    }

    public interface ISupportMultilingualDisplayName<TTranslation>
        where TTranslation : Entity, ISupportDisplayName
    {
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

        public static IEnumerable<string> IsNotNullOrEmpty(this IEnumerable<string> source)
        {
            return source.Where(_ => _.IsNullOrEmpty() == false);
        }

        public static IEnumerable<T> IsNotNull<T>(this IEnumerable<T> source)
        {
            return source.Where(_ => _ != null);
        }

        public static IEnumerable<T> OrderByRandom<T>(this IEnumerable<T> source)
        {
            var randomizer = new Random();
            return source.OrderBy(_ => randomizer.Next());
        }
    }

    public static class IQueryableUtility
    {
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

        public static IQueryable<T> OrderByRandom<T>(this IQueryable<T> query)
        {
            return query.OrderBy(en => EF.Functions.Random());
        }

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

        public static IOrderedQueryable<T> OrderByPropertyName<T>(this IQueryable<T> source, string propertyNamePath)
       => source.OrderByPropertyNameUsing(propertyNamePath, "OrderBy");

        public static IOrderedQueryable<T> OrderByPropertyNameDescending<T>(this IQueryable<T> source, string propertyNamePath)
            => source.OrderByPropertyNameUsing(propertyNamePath, "OrderByDescending");

        public static IOrderedQueryable<T> ThenByPropertyName<T>(this IQueryable<T> source, string propertyNamePath)
            => source.OrderByPropertyNameUsing(propertyNamePath, "ThenBy");

        public static IOrderedQueryable<T> ThenByPropertyNameDescending<T>(this IQueryable<T> source, string propertyNamePath)
            => source.OrderByPropertyNameUsing(propertyNamePath, "ThenByDescending");

        private static IOrderedQueryable<T> OrderByPropertyNameUsing<T>(this IQueryable<T> source, string propertyNamePath, string method)
        {
            var parameter = Expression.Parameter(typeof(T), "item");

            var member = propertyNamePath.Split('.')
                .Aggregate((Expression)parameter, Expression.PropertyOrField);
            
            var keySelector = Expression.Lambda(member, parameter);
            var methodCall = Expression.Call(typeof(Queryable), method, new[]
                    { parameter.Type, member.Type },
                source.Expression, Expression.Quote(keySelector));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery(methodCall);
        }

        public static IQueryable<string> IsNotNullOrEmpty(this IQueryable<string> source)
        {
            return source.Where(_ => _.IsNullOrEmpty() == false);
        }

        public static IQueryable<T> IsNotNull<T>(this IQueryable<T> source)
        {
            return source.Where(_ => _ != null);
        }
    }
}
