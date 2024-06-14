using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public static class SqlServerDbContextHandlerExtensions
    {
        public static void UseDistributedCache(this IHostApplicationBuilder builder)
        {
            builder.Services.AddDistributedPostgreSqlCache((serviceProvider, options) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DbContextConnection");

                options.ConnectionString = connectionString;
                options.SchemaName = "core";
                options.TableName = "Cache";

                options.CreateInfrastructure = true;
                options.DefaultSlidingExpiration = TimeSpan.FromMinutes(15);
            });
        }
    }
}