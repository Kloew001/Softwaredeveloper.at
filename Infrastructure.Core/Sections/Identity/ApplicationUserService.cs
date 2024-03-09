

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity
{
    public class ApplicationUserService : EntityService<ApplicationUser>
    {
        public ApplicationUserService(
            EntityServiceDependency<ApplicationUser> entityServiceDependency)
            : base(entityServiceDependency)
        {
        }

        public async Task<bool> IsInRoleAsync(Guid userId, Guid roleId)
        {
            var isUserIndRole = await _context
                .Set<ApplicationUserRole>()
                    .AnyAsync(_ => _.UserId == userId &&
                        _.RoleId == roleId);

            return isUserIndRole;
        }

        protected override IQueryable<ApplicationUser> AppendOrderBy(IQueryable<ApplicationUser> query)
        {
            return query.OrderBy(_ => _.Email);
        }
    }
}
