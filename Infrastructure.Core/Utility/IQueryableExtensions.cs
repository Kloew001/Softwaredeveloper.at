using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class IQueryableExtensions
{
   
    public static IQueryable<T> OrderByRandom<T>(this IQueryable<T> query)
    {
        return query.OrderBy(en => EF.Functions.Random());
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
