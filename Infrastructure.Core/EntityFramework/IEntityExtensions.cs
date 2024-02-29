using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
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
