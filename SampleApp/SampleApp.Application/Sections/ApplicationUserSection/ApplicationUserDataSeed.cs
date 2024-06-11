using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SampleApp.Application.Sections.ApplicationUserSection
{
    public class ApplicationRoleDataSeed : ApplicationRoleDataSeed<UserRoleType>
    {
        public ApplicationRoleDataSeed(IApplicationUserService applicationUserService)
            : base(applicationUserService)
        {
            AutoExecute = true;
        }
    }

    public class ApplicationUserDataSeed : BaseApplicationUserDataSeed
    {

        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        public ApplicationUserDataSeed(
            IApplicationUserService applicationUserService,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
            : base(applicationUserService)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            AutoExecute = true;
        }

        public override async Task SeedAsync(CancellationToken cancellationToken)
        {
            var user = await _applicationUserService.GetUserInternalById(ApplicationUserIds.ServiceAdminId);
            if (user != null)
                return;

            var pw = _configuration.GetSection("ServiceUser").GetValue<string>("Password");

            using (_serviceProvider.GetService<SectionManager>().CreateSectionScope<SecurityFreeSection>())
            {
                await EnsureUserAsync(ApplicationUserIds.ServiceAdminId,
                                   "ServiceAdmin V",
                                    "ServiceAdmin N",
                                 _configuration.GetSection("ServiceUser").GetValue<string>("EMail"),
                                 pw,
                                [UserRoleType.Admin.GetName()]);
            }
        }
    }
}
