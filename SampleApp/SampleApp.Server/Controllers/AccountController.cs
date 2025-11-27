using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

namespace SoftwaredeveloperDotAt.Server.Controllers;

public class AccountController : BaseAuthenticateController
{
    public AccountController(ITokenAuthenticateService authenticateService) : base(authenticateService)
    {
    }
}
