namespace SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;

[ScopedDependency]
public class AccessConditionService
{
    protected readonly ICurrentUserService _currentUserService;
    protected readonly IApplicationUserService _applicationUserService;
    protected readonly IDbContext _context;

    public AccessConditionService(
        ICurrentUserService currentUserService,
        IApplicationUserService applicationUserService,
        IDbContext context)
    {
        _currentUserService = currentUserService;
        _applicationUserService = applicationUserService;
        _context = context;
    }

    public Guid GetCurrentUserId()
    {
        var currentUserId = _currentUserService.GetCurrentUserId().Value;
        return currentUserId;
    }

    public bool IsServiceAdmin()
    {
        return _currentUserService.IsServiceAdmin();
    }

    public async ValueTask<bool> IsAdminAsync()
    {
        if (IsServiceAdmin())
            return true;

        return await _applicationUserService
            .IsCurrentUserInRoleAsync(UserRoleType.Parse("Admin").Id);
    }
}