using SoftwaredeveloperDotAt.Infrastructure.Core.DataSeed;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity
{
    public class ApplicationRoleDataSeed : IDataSeed
    {
        public decimal Priority => 1.1m;
        public bool AutoExecute => true;

        protected readonly IApplicationUserService _applicationUserService;

        public ApplicationRoleDataSeed(IApplicationUserService applicationUserService)
        {
            _applicationUserService = applicationUserService;
        }

        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            foreach (var userRoleType in UserRoleType.GetAll())
            {
                await _applicationUserService.CreateRoleAsync(
                    userRoleType.Id,
                    userRoleType.Name);
            }
        }
    }

}
