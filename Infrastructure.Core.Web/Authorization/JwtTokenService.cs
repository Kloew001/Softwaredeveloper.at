using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using static SoftwaredeveloperDotAt.Infrastructure.Core.Web.WebApplicationBuilderExtensions;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization;

public interface ITokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims, DateTime expires);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}

public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtBearerOptions _jwtBearearOptions;

    public JwtTokenService(JwtSettings jwtSettings, IOptionsMonitor<JwtBearerOptions> options)
    {
        _jwtSettings = jwtSettings;
        _jwtBearearOptions = options.Get(JwtBearerDefaults.AuthenticationScheme);
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims, DateTime expires)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            SigningCredentials = new SigningCredentials(_jwtBearearOptions.TokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256Signature),
            Issuer = _jwtBearearOptions.TokenValidationParameters.ValidIssuer,
            Audience = _jwtBearearOptions.TokenValidationParameters.ValidAudience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenValidationParameters = _jwtBearearOptions.TokenValidationParameters.Clone();
            tokenValidationParameters.ValidateLifetime = false;

            //new TokenValidationParameters
            //{
            //    ValidateAudience = false,
            //    ValidateIssuer = false,
            //    ValidateIssuerSigningKey = true,
            //    IssuerSigningKey = _jwtBearearOptions.TokenValidationParameters.IssuerSigningKey,
            //    ValidateLifetime = false // here we are saying that we don't care about the token's expiration date
            //};

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

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