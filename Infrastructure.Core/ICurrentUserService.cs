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

    public class CurrentUserService : ICurrentUserService
    {
        private Guid? _currentUserId = ApplicationUserIds.ServiceAdminId;
        private Guid? _previousUserId = null;

        private readonly IDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public bool IsAuthenticated => _currentUserId != null;

        public CurrentUserService(IDbContext context,  IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
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
            var currentUserService = _serviceProvider.GetService<ICurrentLanguageService>();
            currentUserService.Init();
        }

        public void SetCurrentUserId(Guid? currentUserId)
        {
            _currentUserId = currentUserId;
        }
    }
}
