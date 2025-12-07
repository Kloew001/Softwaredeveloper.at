using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

namespace SampleApp.Server.Controllers;

public class ApplicationUserController : BaseApplicationUserController
{
    public ApplicationUserController(IApplicationUserService applicationUserService)
        : base(applicationUserService)
    {
    }
}