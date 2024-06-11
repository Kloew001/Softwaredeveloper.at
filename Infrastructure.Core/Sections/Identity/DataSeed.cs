using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;
using SoftwaredeveloperDotAt.Infrastructure.Core.DataSeed;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity
{
    public abstract class ApplicationRoleDataSeed<TUserRoles> : IDataSeed
        where TUserRoles : struct, Enum
    {
        public decimal Priority => 1.1m;

        public bool AutoExecute { get; set; } = false;

        protected readonly IApplicationUserService _applicationUserService;

        public ApplicationRoleDataSeed(IApplicationUserService applicationUserService)
        {
            _applicationUserService = applicationUserService;
        }

        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            foreach (var roleType in Enum.GetValues<TUserRoles>())
            {
                await _applicationUserService.CreateRoleAsync(
                    roleType.GetId(),
                    roleType.GetName());
            }
        }
    }

    public abstract class BaseApplicationUserDataSeed : IDataSeed
    {
        public virtual decimal Priority => 1.2m;

        public bool AutoExecute { get; set; } = false;


        protected readonly IApplicationUserService _applicationUserService;

        public BaseApplicationUserDataSeed(
            IApplicationUserService applicationUserService)
        {
            _applicationUserService = applicationUserService;
        }

        public abstract Task SeedAsync(CancellationToken cancellationToken);

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
