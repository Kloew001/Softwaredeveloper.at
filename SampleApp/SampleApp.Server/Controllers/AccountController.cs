using Microsoft.AspNetCore.Http.HttpResults;

using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

namespace RWA.Server.Controllers;

public class AccountController : BaseApiController
{
    private readonly ITokenAuthenticateService _authenticateService;

    public AccountController(ITokenAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    [HttpPost]
    [AllowAnonymous]
    public Task<Results<
        Ok<AccessTokenResponse>,
        EmptyHttpResult,
        UnauthorizedHttpResult,
        ProblemHttpResult>> AuthenticateToken
               ([FromBody] AuthenticatePasswordRequest request)
        => _authenticateService.AuthenticateTokenAsync(request);

    [HttpPost]
    [AllowAnonymous]
    public Task<Results<Ok<AccessTokenResponse>, BadRequest>> RefreshToken
        ([FromBody] RefreshRequest refreshRequest)
        => _authenticateService.RefreshToken(refreshRequest);

    [HttpPost]
    public Task RevokeToken()
         => _authenticateService.RevokeToken();
}
