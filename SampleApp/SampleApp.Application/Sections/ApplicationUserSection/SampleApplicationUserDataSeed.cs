
using Microsoft.Extensions.Configuration;

namespace SampleApp.Application.Sections.ApplicationUserSection
{
    public class SampleApplicationUserDataSeed : ApplicationUserDataSeed
    {
        public SampleApplicationUserDataSeed(
            IApplicationUserService applicationUserService,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
            : base(applicationUserService, serviceProvider, configuration)
        {
        }
    }
}