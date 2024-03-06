using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        public async Task SeedAsync()
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

        public bool ExecuteInThread { get; set; } = false;

        public bool AutoExecute { get; set; } = false;


        private readonly UserManager<ApplicationUser> _userManager;

        public BaseApplicationUserDataSeed(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public abstract Task SeedAsync();

        protected async Task<ApplicationUser> EnsureUserAsync(
            Guid id,
            string vorname,
            string nachname,
            string username,
            string password,
            string roleName)
        {
            var user = _userManager.FindByNameAsync(username).GetAwaiter().GetResult();

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = id,
                    UserName = username,
                    Email = username,
                    EmailConfirmed = true,
                    FirstName = vorname,
                    LastName = nachname,
                    DateCreated = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    var role = await _userManager.AddToRoleAsync(user, roleName);
                }
            }
            else
            {
            }

            return user;
        }

    }

}
