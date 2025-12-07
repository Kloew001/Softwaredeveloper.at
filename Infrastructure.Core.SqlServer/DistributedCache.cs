using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.SqlServer;

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