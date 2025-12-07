using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

public abstract class BaseAuthenticateController : BaseApiController
{
    protected readonly ITokenAuthenticateService _authenticateService;

    public BaseAuthenticateController(ITokenAuthenticateService authenticateService)
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