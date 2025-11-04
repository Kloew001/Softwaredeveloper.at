using Microsoft.AspNetCore.Mvc;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

public abstract class BaseApplicationUserController : BaseApiController
{
    private readonly IApplicationUserService _applicationUserService;

    public BaseApplicationUserController(IApplicationUserService applicationUserService)
    {
        _applicationUserService = applicationUserService;
    }

    [HttpGet]
    public Task<ApplicationUserDetailDto> GetCurrentUser()
        => _applicationUserService.GetCurrentUserAsync();
}
