
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SampleApp.Application
{
    public class DomainStartup : StartupCore<ApplicationSettings>
    {
        public override void ConfigureServices(IHostApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;
            var hostEnvironment = builder.Environment;

            base.ConfigureServices(builder);

            services.AddDbContext<SampleAppContext>((serviceProvider, options) =>
            {
                var dbContextHandler = serviceProvider.GetRequiredService<IDbContextHandler>();
                dbContextHandler.DBContextOptions(serviceProvider, options);
            });
        }
    }
}
