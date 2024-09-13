using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public static class ICurrentUserServiceExtensions
    {
        public static Guid? GetCurrentUserId<TEntity>(this EntityService<TEntity> entityService)
            where TEntity : Entity
        {
            var service =
                entityService.EntityServiceDependency.ServiceProvider
                    .GetRequiredService<ICurrentUserService>();

            return service.GetCurrentUserId();
        }
    }

    public interface ICurrentUserService
    {
        Guid? GetCurrentUserId();
        void SetCurrentUserId(Guid? currentUserId);
        bool IsAuthenticated { get; }
    }

    public class AlwaysServiceUserCurrentUserService : ICurrentUserService
    {
        public bool IsAuthenticated => true;

        public Guid? GetCurrentUserId() => ApplicationUserIds.ServiceAdminId;

        public void SetCurrentUserId(Guid? currentUserId) { }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private Guid? _currentUserId = ApplicationUserIds.ServiceAdminId;
        private Guid? _previousUserId = null;

        private readonly IDbContext _context;

        public bool IsAuthenticated => _currentUserId != null;

        public CurrentUserService(IDbContext context)
        {
            _context = context;
        }

        public void SetPreviousUser()
        {
            if (_previousUserId != null)
                SetCurrentUser(_previousUserId.Value);

            _previousUserId = null;
        }

        public Guid? GetCurrentUserId()
        {
            return _currentUserId.HasValue ? _currentUserId.Value : null;
        }

        public ApplicationUser GetCurrentUser()
        {
            return _context.Set<ApplicationUser>().SingleOrDefault(u => u.Id == _currentUserId);
        }

        public void SetCurrentUser(Guid? id)
        {
            _previousUserId = _currentUserId;
            _currentUserId = id;
        }

        public void SetCurrentUserId(Guid? currentUserId)
        {
            _currentUserId = currentUserId;
        }
    }
}
