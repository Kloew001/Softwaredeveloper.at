using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;

using static SoftwaredeveloperDotAt.Infrastructure.Core.Web.WebApplicationBuilderExtensions;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization;

public class AuthenticateTokenRequest
{
    public string UserName { get; set; }
    public string Email { get; set; }
}

public class AuthenticatePasswordRequest
{
    public string UserName { get; set; }
    public string Email { get; set; }

    public string Password { get; set; }
}

public class AccessTokenResponse
{
    public string AccessToken { get; init; }
    public long ExpiresAt { get; init; }
    public string RefreshToken { get; init; }
}
public class RefreshRequest
{
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
}

//https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs
public class TokenAuthenticateService : ITokenAuthenticateService
{
    public const string TokenProviderName = "TokenProvider";
    private const string RefreshTokenName = "RefreshToken";

    protected readonly SignInManager<ApplicationUser> _signInManager;
    protected readonly UserManager<ApplicationUser> _userManager;
    protected readonly JwtSettings _jwtSettings;
    protected readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;
    protected readonly ICurrentUserService _currentUserService;
    protected readonly ITokenService _tokenService;
    protected readonly TimeProvider _timeProvider;

    public TokenAuthenticateService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        JwtSettings jwtSettings,
        ITokenService tokenService,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
        TimeProvider timeProvider)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtSettings = jwtSettings;
        _bearerTokenOptions = bearerTokenOptions;
        _currentUserService = currentUserService;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
    }

    public virtual async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, UnauthorizedHttpResult, ProblemHttpResult>>
        AuthenticateTokenAsync(AuthenticateTokenRequest request)
    {
        _signInManager.AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
        ApplicationUser user = null;

        if (request.UserName.IsNotNullOrEmpty())
            user = await _userManager.FindByNameAsync(request.UserName);

        else if (request.Email.IsNotNullOrEmpty())
            user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return TypedResults.Unauthorized();

        if (!await CanUserAuthenticateAsync(user))
        {
            return TypedResults.Unauthorized();
        }

        if (!await _signInManager.CanSignInAsync(user))
        {
            return TypedResults.Unauthorized();
        }

        if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
        {
            return TypedResults.Unauthorized();
        }

        var accessTokenResponse = GetAccessTokenResponse(user);

        await SetRefreshToken(user, accessTokenResponse.RefreshToken);

        return TypedResults.Ok(accessTokenResponse);
    }

    public virtual async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, UnauthorizedHttpResult, ProblemHttpResult>>
        AuthenticateTokenAsync(AuthenticatePasswordRequest request)
    {
        _signInManager.AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;

        ApplicationUser user = null;

        if (request.UserName.IsNotNullOrEmpty())
            user = await _userManager.FindByNameAsync(request.UserName);

        else if (request.Email.IsNotNullOrEmpty())
            user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return TypedResults.Unauthorized();

        if (request.Password.IsNullOrEmpty())
            return TypedResults.Unauthorized();

        if (!await CanUserAuthenticateAsync(user))
        {
            return TypedResults.Unauthorized();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
            return TypedResults.Unauthorized();

        return await AuthenticateTokenAsync(new AuthenticateTokenRequest() { UserName = request.UserName, Email = request.Email });
    }

    protected virtual ValueTask<bool> CanUserAuthenticateAsync(ApplicationUser user)
    {
        return ValueTask.FromResult(user.IsEnabled);
    }

    private static List<Claim> GetClaims(ApplicationUser user)
    {
        return
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtSecurityStampClaim, user.SecurityStamp)
        ];
    }

    public const string JwtSecurityStampClaim = "secstamp";
    public const string RefreshTokenExpiryClaim = "refreshTokenExpiry";

    private AccessTokenResponse GetAccessTokenResponse(ApplicationUser user)
    {
        var claims = GetClaims(user);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var accessToken = _tokenService.GenerateAccessToken(claims, expires);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var accessTokenResponse = new AccessTokenResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expires.ToUniversalTime().Ticks,
            RefreshToken = refreshToken
        };

        return accessTokenResponse;
    }

    public async Task<IResult> RevokeToken()
    {
        var userId = _currentUserService.GetCurrentUserId()?.ToString();

        if (userId == null)
            return TypedResults.Challenge();

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return TypedResults.Challenge();

        await _userManager.RemoveAuthenticationTokenAsync(user, TokenProviderName, RefreshTokenName);

        var expiryClaim = await GetRefreshTokenExpiryClaimAsync(user);
        if (expiryClaim != null)
            await _userManager.RemoveClaimAsync(user, expiryClaim);

        await _userManager.UpdateSecurityStampAsync(user);

        return TypedResults.Ok();
    }

    public virtual async Task<Results<Ok<AccessTokenResponse>, BadRequest>> RefreshToken
        (RefreshRequest refreshRequest)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(refreshRequest.AccessToken);
        var claims = principal.Claims.ToList();

        var userId = claims?.FirstOrDefault(_ => _.Type == JwtRegisteredClaimNames.Sub || _.Type == ClaimTypes.NameIdentifier)?.Value;

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return TypedResults.BadRequest();

        var (refreshToken, expires) = await GetRefreshTokenAsync(user);

        if (refreshToken == null || !expires.HasValue || expires.Value < DateTimeOffset.UtcNow)
            return TypedResults.BadRequest();

        if (refreshToken != refreshRequest.RefreshToken)
            return TypedResults.BadRequest();

        var accessTokenResponse = GetAccessTokenResponse(user);
        await SetRefreshToken(user, accessTokenResponse.RefreshToken);

        return TypedResults.Ok(accessTokenResponse);
    }

    private async Task SetRefreshToken(ApplicationUser user, string refreshToken)
    {
        await _userManager.RemoveAuthenticationTokenAsync(user, TokenProviderName, RefreshTokenName);
        await _userManager.SetAuthenticationTokenAsync(user, TokenProviderName, RefreshTokenName, refreshToken);

        var expiryClaim = await GetRefreshTokenExpiryClaimAsync(user);
        if (expiryClaim != null)
            await _userManager.RemoveClaimAsync(user, expiryClaim);

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationMinutes);

        expiryClaim = new Claim(RefreshTokenExpiryClaim, expires.ToString("o")); // "o" formats DateTime as ISO 8601

        await _userManager.AddClaimAsync(user, expiryClaim);
    }

    private async Task<(string refreshToken, DateTimeOffset? expires)> GetRefreshTokenAsync(ApplicationUser user)
    {
        // Get the refresh token
        var refreshToken = await _userManager.GetAuthenticationTokenAsync(user, TokenProviderName, RefreshTokenName);

        var expiryClaim = await GetRefreshTokenExpiryClaimAsync(user);

        DateTimeOffset? expires = null;
        if (expiryClaim != null)
        {
            if (DateTimeOffset.TryParse(expiryClaim.Value, out var parsedExpiry))
            {
                expires = parsedExpiry;
            }
        }

        return (refreshToken, expires);
    }

    private async Task<Claim> GetRefreshTokenExpiryClaimAsync(ApplicationUser user)
    {
        return (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == RefreshTokenExpiryClaim);
    }
}