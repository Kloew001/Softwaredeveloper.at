using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity
{
    public class WebApplicationUserIdentityService : IApplicationUserIdentityService, ITypedScopedDependency<IApplicationUserIdentityService>
    {
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly IUserStore<ApplicationUser> _userStore;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IDateTimeService _dateTimeService;

        public WebApplicationUserIdentityService(IServiceProvider serviceProvider)
        {
            _signInManager = serviceProvider.GetService<SignInManager<ApplicationUser>>();
            _userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            _userStore = serviceProvider.GetService<IUserStore<ApplicationUser>>();
            _roleManager = serviceProvider.GetService<RoleManager<ApplicationRole>>();
            _dateTimeService = serviceProvider.GetService<IDateTimeService>();
        }

        public async Task<Guid> CreateRoleAsync(Guid id, string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Id = id,
                    Name = roleName,
                });
            }

            return id;
        }

        public async Task<Guid> CreateUserAsync(CreateApplicationUserIdentity identity, CancellationToken ct)
        {
            ApplicationUser user = null;

            if (identity.UserName.IsNotNullOrEmpty())
                user = await _userManager.FindByEmailAsync(identity.UserName);

            else if (identity.Email.IsNotNullOrEmpty())
                user = await _userManager.FindByEmailAsync(identity.Email);

            if (user != null)
                return user.Id;

            user = new ApplicationUser();

            await _userStore.SetUserNameAsync(user, identity.UserName, ct);
            await ((IUserEmailStore<ApplicationUser>)_userStore)
                .SetEmailAsync(user, identity.Email, ct);

            user.Id = identity.Id;
            user.FirstName = identity.FirstName;
            user.LastName = identity.LastName;
            user.IsEnabled = true;
            user.DateCreated = _dateTimeService.Now();

            user.EmailConfirmed = identity.EmailConfirmed;

            IdentityResult result;

            if (identity.Password.IsNotNullOrEmpty())
                result = await _userManager.CreateAsync(user, identity.Password);
            else
                result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                if (identity.RoleNames != null)
                {
                    foreach (var roleName in identity.RoleNames)
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                    }
                }

                return user.Id;
            }

            var error = string.Join(",", result.Errors.Select(_ => _.Description));
            throw new Exception(error);
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return;

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new Exception(result.Errors.First().Description);
        }
    }
}
