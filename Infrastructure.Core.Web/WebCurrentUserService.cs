using DocumentFormat.OpenXml.Spreadsheet;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

using SoftwaredeveloperDotAt.Infrastructure.Core;

using System.Security.Claims;

namespace Infrastructure.Core.Web
{
    public class WebCurrentUserService : ICurrentUserService
    {
        protected IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentLanguageService _currentLanguageService;

        public WebCurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentLanguageService = serviceProvider.GetService<ICurrentLanguageService>();
        }

        public bool IsAuthenticated
        {
            get
            {
                if (_currentUserId.HasValue == false)
                {
                    try
                    {
                        GetCurrentUserId();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return false;
                    }
                }

                return _currentUserId.HasValue;
            }
        }
        private Guid? _currentUserId;

        public Guid? GetCurrentUserId()
        {
            if (_currentUserId.HasValue)
                return _currentUserId.Value;

            _currentUserId = ResolveCurrentUserId();

            return _currentUserId;
        }

        protected virtual Guid? ResolveCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var claims = user?.Claims;
            //var userClaims = user?.Claims.Select(c => new { c.Type, c.Value }).ToList();

            var id = claims?.FirstOrDefault(_ =>
                _.Type == JwtRegisteredClaimNames.Sub ||
                _.Type == ClaimTypes.NameIdentifier)?.Value;

            if (id != null)
                return Guid.Parse(id);

            return null;
        }

        public void SetCurrentUserId(Guid? currentUserId)
        {
            _currentUserId = currentUserId;
        }
    }
}
