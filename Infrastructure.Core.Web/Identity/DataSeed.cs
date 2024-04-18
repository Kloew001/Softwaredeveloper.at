using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using SoftwaredeveloperDotAt.Infrastructure.Core.DataSeed;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity
{
    public abstract class ApplicationRoleDataSeed<TUserRoles> : IDataSeed
        where TUserRoles : struct, Enum
    {
        public decimal Priority => 1.1m;

        public bool ExecuteInThread { get; set; } = false;

        public bool AutoExecute { get; set; } = false;


        private readonly RoleManager<ApplicationRole> _roleManager;

        public ApplicationRoleDataSeed(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            foreach (var roleType in Enum.GetValues<TUserRoles>())
            {
                if (!await _roleManager.RoleExistsAsync(roleType.GetName()))
                {
                    await _roleManager.CreateAsync(new ApplicationRole
                    {
                        Id = roleType.GetId(),
                        Name = roleType.GetName(),
                    });
                }
            }
        }
    }

    public abstract class BaseApplicationUserDataSeed : IDataSeed
    {
        public decimal Priority => 1.2m;

        public bool AutoExecute { get; set; } = false;


        protected readonly IApplicationUserService _applicationUserService;

        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseApplicationUserDataSeed(
            UserManager<ApplicationUser> userManager,
            IApplicationUserService applicationUserService)
        {
            _userManager = userManager;
            _applicationUserService = applicationUserService;
        }

        public abstract Task SeedAsync(CancellationToken cancellationToken);

        protected async Task<ApplicationUser> EnsureUserAsync(
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

            return await _userManager.FindByIdAsync(id.ToString());
        }

    }

}
