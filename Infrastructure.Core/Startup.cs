using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public class StartupCore
    {
        protected IConfiguration _configuration;
        protected IHostEnvironment _hostEnvironment;
        protected IServiceCollection _services;

        public virtual void ConfigureServices(
            IServiceCollection services,
            IHostEnvironment hostEnvironment,
            IConfiguration configuration)
        {
            _services = services;
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;

            services.RegisterSelfRegisterServices();
            services.RegisterAllHostedService();
        }

        public virtual void ConfigureApp(IHost host)
        {
            host.UseDtoFactory();   
        }
    }
}
