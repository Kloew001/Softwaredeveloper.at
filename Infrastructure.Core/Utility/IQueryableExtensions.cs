using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

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

public static class DbContextLocalExtensions
{
    public static T? FindOrDefault<T>(
        this IDbContext context,
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? includes = null)
        where T : class
    {
        var set = context.Set<T>();
        if (includes is null)
        {
            var local = set.Local.SingleOrDefault(predicate.Compile());
            if (local is not null)
                return local;
        }

        var query = includes is null ? set : includes(set);

        return query.SingleOrDefault(predicate);
    }

    public static async Task<T?> FindOrDefaultAsync<T>(
        this IDbContext context,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default,
        Func<IQueryable<T>, IQueryable<T>>? includes = null)
        where T : class
    {
        var set = context.Set<T>();
        if (includes is null)
        {
            var local = set.Local.SingleOrDefault(predicate.Compile());
            if (local is not null)
                return local;
        }

        var query = includes is null ? set : includes(set);

        return await query.SingleOrDefaultAsync(predicate, cancellationToken);
    }
}