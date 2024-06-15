using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;
using SoftwaredeveloperDotAt.Infrastructure.Core.DataSeed;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity
{
    public abstract class ApplicationUserDataSeed : IDataSeed
    {
        public virtual decimal Priority => 1.2m;
        public bool AutoExecute => true;

        protected readonly IApplicationUserService _applicationUserService;
        protected readonly IConfiguration _configuration;
        protected readonly IServiceProvider _serviceProvider;

        public ApplicationUserDataSeed(
            IApplicationUserService applicationUserService,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _applicationUserService = applicationUserService;

            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public virtual async Task SeedAsync(CancellationToken cancellationToken)
        {
            using (_serviceProvider.GetService<SectionManager>().CreateSectionScope<SecurityFreeSection>())
            {
                var user = await _applicationUserService.GetUserInternalById(ApplicationUserIds.ServiceAdminId);
                if (user != null)
                    return;

                var serviceUser = _configuration.GetSection("ServiceUser");

                if (serviceUser == null)
                    return;

                var email = serviceUser.GetValue<string>("EMail");
                var pw = serviceUser.GetValue<string>("Password");

                await EnsureUserAsync(ApplicationUserIds.ServiceAdminId,
                                   "ServiceAdmin V", "ServiceAdmin N",
                                 email, pw,
                                ["Admin"]);
            }
        }

        protected async Task<Guid> EnsureUserAsync(
            Guid id,
            string vorname,
            string nachname,
            string username,
            string password,
            string[] roleNames)
        {
            id = (await _applicationUserService.CreateIdentityInternalAsync(new CreateApplicationUserIdentity
            {
                Id = id,
                FirstName = vorname,
                LastName = nachname,
                UserName = username,
                Email = username,
                Password = password,
                EmailConfirmed = true,
                RoleNames = roleNames
            })).Id;

            return id;
        }

    }

}
