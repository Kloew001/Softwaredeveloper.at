using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;


namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity
{
    public class RolesAuthorizationRequirement : IAuthorizationRequirement
    {
        public IEnumerable<string> AllowedRoles { get; }
        public RolesAuthorizationRequirement(params string[] roles)
        {
            AllowedRoles = roles;
        }
    }

    [ScopedDependency<IAuthorizationHandler>]
    public class RolesAuthorizationRequirementHandler : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        private readonly IDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationUserService _applicationUserService;

        public RolesAuthorizationRequirementHandler(IDbContext dbContext,
            IMemoryCache memoryCache,
            ICurrentUserService currentUserService,
            ApplicationUserService applicationUserService)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
            _currentUserService = currentUserService;
            _applicationUserService = applicationUserService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RolesAuthorizationRequirement requirement)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();

            if (currentUserId == null)
                return;

            var isUserIndRole = await _memoryCache.GetOrCreateAsync(
                   $"{nameof(RolesAuthorizationRequirementHandler)}_{currentUserId}_{string.Join(";", requirement.AllowedRoles)}",
                   async entry =>
                   {
                       entry.SlidingExpiration = TimeSpan.FromMinutes(5);

                       foreach (var allowedRole in requirement.AllowedRoles)
                       {
                           var roleId = await _dbContext.Set<ApplicationRole>()
                                       .Where(_ => _.NormalizedName == allowedRole.Normalize().ToUpper())
                                       .Select(_ => _.Id)
                                       .SingleAsync();

                           var isUserIndRole = await
                           _applicationUserService.IsInRoleAsync(currentUserId.Value, roleId);

                           if (isUserIndRole)
                               return true;
                       }

                       return false;
                   });

            if (isUserIndRole)
            {
                context.Succeed(requirement);
            }
        }
    }
}
