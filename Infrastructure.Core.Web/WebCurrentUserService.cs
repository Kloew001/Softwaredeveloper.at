using Microsoft.AspNetCore.Http;

using SoftwaredeveloperDotAt.Infrastructure.Core;

using System.Security.Claims;

namespace Infrastructure.Core.Web
{
    public class WebCurrentUserService : ICurrentUserService
    {
        private IHttpContextAccessor _httpContextAccessor;

        public WebCurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider provider)
        {
            _httpContextAccessor = httpContextAccessor;
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

            var claims = _httpContextAccessor.HttpContext?.User?.Claims;

            var id = claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            if (id != null)
                _currentUserId = Guid.Parse(id);

            return _currentUserId;
        }

        public void SetCurrentUserId(Guid? currentUserId)
        {
            _currentUserId = currentUserId;
        }
    }
}
