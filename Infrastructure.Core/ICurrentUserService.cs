using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface ICurrentUserService
    {
        Guid GetCurrentUserId();
        void SetCurrentUserId(Guid currentUserId);
        bool IsAuthenticated { get; }
    }

    public interface IApplicationIdentitDbContext
    {
        DbSet<ApplicationUser> ApplicationUsers { get; set; }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private Guid _currentUserId = ApplicationUserIds.ServiceAdminId;
        private Guid? _previousUserId = null;

        private IApplicationIdentitDbContext _context;

        public bool IsAuthenticated => _currentUserId != null;

        public CurrentUserService(IApplicationIdentitDbContext context)
        {
            _context = context;
        }

        public void SetPreviousUser()
        {
            if (_previousUserId != null)
                _currentUserId = _previousUserId.Value;

            _previousUserId = null;
        }

        public Guid GetCurrentUserId()
        {
            return _currentUserId;
        }

        public ApplicationUser GetCurrentUser()
        {
            return _context.ApplicationUsers.SingleOrDefault(u => u.Id == _currentUserId);
        }

        public void SetCurrentUser(Guid id)
        {
            _previousUserId = _currentUserId;
            _currentUserId = id;
        }

        public void SetCurrentUserId(Guid currentUserId)
        {
            _currentUserId = currentUserId;
        }
    }
}
