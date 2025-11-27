using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

namespace SoftwaredeveloperDotAt.Server.Controllers;

public class ApplicationUserController : BaseApplicationUserController
{
    public ApplicationUserController(IApplicationUserService applicationUserService)
        : base(applicationUserService)
    {
    }
}
