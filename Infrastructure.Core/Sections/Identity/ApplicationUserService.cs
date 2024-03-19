

using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity
{
    public class ApplicationUserService : EntityService<ApplicationUser>
    {
        public ApplicationUserService(
            EntityServiceDependency<ApplicationUser> entityServiceDependency)
            : base(entityServiceDependency)
        {
        }

        public Task<ApplicationUserDetailDto> GetCurrentUserAsync()
        {
            var currentUserId = _currentUserService.GetCurrentUserId();

            return GetSingleByIdAsync<ApplicationUserDetailDto>(currentUserId.Value);
        }

        public Task<bool> IsCurrentUserInRoleAsync(params Guid[] roleIds)
        {
            return IsInRoleAsync(_currentUserService.GetCurrentUserId().Value, roleIds);
        }

        public async Task<bool> IsInRoleAsync(Guid userId, params Guid[] roleIds)
        {
            var isUserIndRole = await _context
                .Set<ApplicationUserRole>()
                    .AnyAsync(_ => _.UserId == userId &&
                        roleIds.Contains(_.RoleId));

            return isUserIndRole;
        }

        protected override IQueryable<ApplicationUser> AppendOrderBy(IQueryable<ApplicationUser> query)
        {
            return query.OrderBy(_ => _.Email);
        }
    }
}
