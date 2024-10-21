using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

public static class PostgreSQLContextHandlerExtensions
{
    public static void UseDistributedCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDistributedPostgreSqlCache((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DbContextConnection");

            options.ConnectionString = connectionString;
            options.SchemaName = "core";
            options.TableName = "Cache";

            options.CreateInfrastructure = true;
            options.DefaultSlidingExpiration = TimeSpan.FromMinutes(15);
        });
    }
}