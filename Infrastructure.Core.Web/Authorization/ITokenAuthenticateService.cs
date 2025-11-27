using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization;

public interface ITokenAuthenticateService
{
    Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, UnauthorizedHttpResult, ProblemHttpResult>> AuthenticateTokenAsync(AuthenticatePasswordRequest request);
    Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, UnauthorizedHttpResult, ProblemHttpResult>> AuthenticateTokenAsync(AuthenticateTokenRequest request);
    Task<Results<Ok<AccessTokenResponse>, BadRequest>> RefreshToken(RefreshRequest refreshRequest);
    Task<IResult> RevokeToken();
}