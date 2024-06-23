using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public static class DistributedCacheExtensions
    {
        public static void UseDistributedCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDistributedSqlServerCache(options =>
            {
                var connectionString = configuration.GetConnectionString("DbContextConnection");

                options.ConnectionString = connectionString;
                options.SchemaName = "core";
                options.TableName = "Cache";

                options.DefaultSlidingExpiration = TimeSpan.FromMinutes(15);
            });
        }
    }
}