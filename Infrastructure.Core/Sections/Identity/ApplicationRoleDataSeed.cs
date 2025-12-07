using SoftwaredeveloperDotAt.Infrastructure.Core.DataSeed;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;

public class ApplicationRoleDataSeed : IDataSeed
{
    public decimal Priority => 1.1m;
    public bool AutoExecute => true;

    protected readonly IApplicationUserService _applicationUserService;

    public ApplicationRoleDataSeed(IApplicationUserService applicationUserService)
    {
        _applicationUserService = applicationUserService;
    }

    public virtual async Task SeedAsync(CancellationToken cancellationToken)
    {
        var userRoleTypes = UserRoleType.GetAll();
        foreach (var userRoleType in userRoleTypes)
        {
            await _applicationUserService.EnsureRoleAsync(
                userRoleType.Id,
                userRoleType.Name);
        }
    }
}