using Microsoft.AspNetCore.Http.HttpResults;

using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

namespace RWA.Server.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly AccountService _accountService;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(AccountService accountService, IHttpContextAccessor httpContextAccessor)
        {
            _accountService = accountService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [AllowAnonymous]
        public Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, UnauthorizedHttpResult, ProblemHttpResult>> AuthenticateToken
                   ([FromBody] AuthenticateTokenRequest request)
            => _accountService.AuthenticateToken(HttpContext, request);

        [HttpPost]
        public Task<Results<Ok<AccessTokenResponse>,  ChallengeHttpResult>> RefreshToken
            ([FromBody] RefreshRequest refreshRequest)
            => _accountService.RefreshToken(refreshRequest);
       
        [HttpPost]
        public Task RevokeToken
             ()
             => _accountService.RevokeToken();

        [HttpGet]
        public IActionResult GetUserClaims()
        {
            var userClaims = _httpContextAccessor.HttpContext?.User?.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(userClaims);
        }
    }
}
