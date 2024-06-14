
using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SampleApp.Application
{
    public class DomainStartup : DomainStartupCore<ApplicationSettings>
    {
        public override void ConfigureServices(IHostApplicationBuilder builder)
        {
            var services = builder.Services;

            base.ConfigureServices(builder);

            builder.Services.AddSingleton<IDbContextHandler, PostgreSQLDbContextHandler>();
            services.RegisterDBContext<SampleAppContext>();

            services.AddDistributedPostgreSqlCache((serviceProvider, setup) =>
            {
                var connectionString = serviceProvider.GetService<IApplicationSettings>()
                    .ConnectionStrings["DbContextConnection"];

                setup.ConnectionString = connectionString;
                setup.SchemaName = "core";
                setup.TableName = "Cache";

                setup.CreateInfrastructure = true;
            });
        }
    }
}
