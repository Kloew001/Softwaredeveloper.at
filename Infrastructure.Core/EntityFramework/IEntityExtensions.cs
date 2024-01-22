using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AutoQueryIncludeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OrderByDefaultAttribute : Attribute
    {
        public enum SortDirection
        {
            Ascending,
            Descending
        }
        public int Order { get; set; }
        public SortDirection Direction { get; set; }

        public OrderByDefaultAttribute(int order = 1, SortDirection direction = SortDirection.Ascending)
        {
            Order = order;
            Direction = direction;
        }
    }

    public static class IEntityExtensions
    {
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
    }
}
