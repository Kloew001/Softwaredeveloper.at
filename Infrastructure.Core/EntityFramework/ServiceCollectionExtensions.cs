using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static void RegisterDBContext<TDBContext>(this IServiceCollection services, string connectionStringKey = "DbContextConnection")
      where TDBContext : DbContext
    {
        services.AddDbContext<TDBContext>((serviceProvider, options) =>
        {
            var dbContextHandler = serviceProvider.GetRequiredService<IDbContextHandler>();
            dbContextHandler.DBContextOptions(serviceProvider, options, connectionStringKey);
        });
    }
}
