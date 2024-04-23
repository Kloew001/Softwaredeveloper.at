using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers
{
    public abstract class BaseAccountController : BaseApiController
    {
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseAccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public class AuthenticateRequest
        {
            public required string Email { get; set; }
            public required string Password { get; set; }
        }

        [HttpPost]
        [AllowAnonymous]
        public virtual async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Authenticate
            ([FromBody] AuthenticateRequest request)
        {
            _signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, lockoutOnFailure: true);

            if (!result.Succeeded || request.Password.IsNullOrEmpty())
            {
                if (result.IsLockedOut)
                {
                    return TypedResults.Problem("Der Benutzer ist gesperrt.", statusCode: StatusCodes.Status401Unauthorized);
                }

                return TypedResults.Problem("Bitte prüfen Sie Username und Passwort.", statusCode: StatusCodes.Status401Unauthorized);
            }

            return TypedResults.Empty;
        }


    }
}
