using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization
{
    public class AuthenticateTokenRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
    public class AccessTokenResponse
    {
        public string AccessToken { get; init; }
        public long ExpiresIn { get; init; }
        public string RefreshToken { get; init; }
    }
    public class RefreshRequest
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
    }

    //https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs
    public class AccountService : IScopedDependency
    {
        public const string TokenProviderName = "TokenProvider";
        private const string RefreshTokenName = "RefreshToken";

        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly IConfiguration _configuration;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;
        protected readonly ICurrentUserService _currentUserService;
        protected readonly ITokenService _tokenService;
        protected readonly TimeProvider _timeProvider;

        public AccountService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IConfiguration configuration,
            ITokenService tokenService,
            IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
            TimeProvider timeProvider)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _bearerTokenOptions = bearerTokenOptions;
            _currentUserService = currentUserService;
            _tokenService = tokenService;
            _timeProvider = timeProvider;
        }

        public virtual async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, UnauthorizedHttpResult, ProblemHttpResult>>
            AuthenticateToken(HttpContext context, AuthenticateTokenRequest request)
        {
            _signInManager.AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;

            var s = _userManager.SupportsUserSecurityStamp;

            request.Email = "office@softwaredeveloper.at";
            request.Password = "94V5jP9?mFxJ99vH8T@m";

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return TypedResults.Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded || request.Password.IsNullOrEmpty())
            {
                if (result.IsLockedOut)
                {
                    return TypedResults.Problem("Der Benutzer ist gesperrt.", statusCode: StatusCodes.Status401Unauthorized);
                }

                return TypedResults.Unauthorized();
            }

            var claims = GetClaims(user);
            var accessTokenResponse = GetAccessTokenResponse(claims);

            await _userManager.RemoveAuthenticationTokenAsync(user, TokenProviderName, RefreshTokenName);
            await _userManager.SetAuthenticationTokenAsync(user, TokenProviderName, RefreshTokenName,
                accessTokenResponse.RefreshToken);

            return TypedResults.Ok(accessTokenResponse);
        }

        private static List<Claim> GetClaims(ApplicationUser user)
        {
            return new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtSecurityStamp, user.SecurityStamp)
            };
        }

        public const string JwtSecurityStamp = "secstamp";

        private AccessTokenResponse GetAccessTokenResponse(List<Claim> claims)
        {
            var expiresInMinutes = _configuration.GetValue<int>("Jwt:ExpiresInMinutes");

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var accessTokenResponse = new AccessTokenResponse
            {
                AccessToken = accessToken,
                ExpiresIn = (long) TimeSpan.FromMinutes(expiresInMinutes).TotalSeconds,
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

            await _userManager.UpdateSecurityStampAsync(user);

            return TypedResults.Ok();
        }

        public virtual async Task<Results<Ok<AccessTokenResponse>, ChallengeHttpResult>> RefreshToken
            (RefreshRequest refreshRequest)
        {
            var userId = _currentUserService.GetCurrentUserId();

            if (userId == null)
                return TypedResults.Challenge();

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return TypedResults.Challenge();

            var refreshToken = await _userManager.GetAuthenticationTokenAsync(user, TokenProviderName, RefreshTokenName);

            if (refreshToken != refreshRequest.RefreshToken)
                return TypedResults.Challenge();
            
            var validateToken = _tokenService.ValidateToken(refreshRequest.AccessToken);
            var claims = validateToken.Claims.ToList();

            var id = claims?.FirstOrDefault(_ => _.Type == JwtRegisteredClaimNames.Sub || _.Type == ClaimTypes.NameIdentifier)?.Value;

            if (user.Id.ToString() != id)
                return TypedResults.Challenge();

            var accessTokenResponse = GetAccessTokenResponse(claims);

            await _userManager.RemoveAuthenticationTokenAsync(user, TokenProviderName, RefreshTokenName);
            await _userManager.SetAuthenticationTokenAsync(user, TokenProviderName, RefreshTokenName,
                accessTokenResponse.RefreshToken);

            return TypedResults.Ok(accessTokenResponse);
        }
    }

    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal ValidateToken(string token);
    }
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly JwtBearerOptions _options;

        public JwtTokenService(IConfiguration configuration, IOptionsMonitor<JwtBearerOptions> options)
        {
            _configuration = configuration;
            _options = options.Get(JwtBearerDefaults.AuthenticationScheme);
        }

        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var expiresInMinutes = _configuration.GetValue<int>("Jwt:ExpiresInMinutes");
            var expires = DateTime.UtcNow.AddMinutes(expiresInMinutes);

            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenValidationParameters = _options.TokenValidationParameters;
                
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                var jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                throw new ApplicationException("Token has expired.");
            }
        }
    }
}
