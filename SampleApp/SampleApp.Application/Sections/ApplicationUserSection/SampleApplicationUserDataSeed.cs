
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SampleApp.Application.Sections.ApplicationUserSection
{
    public class SampleApplicationUserDataSeed : ApplicationUserDataSeed
    {
        public SampleApplicationUserDataSeed(
            IApplicationUserService applicationUserService,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IHostEnvironment environment)
            : base(applicationUserService, serviceProvider, configuration, environment)
        {
        }
    }
}