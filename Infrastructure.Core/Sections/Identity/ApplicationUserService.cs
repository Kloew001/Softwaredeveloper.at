

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.Utility.Cache;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity
{
    public static class IApplicationUserServiceExtensions
    {
        public static Task<bool> IsCurrentUserInRoleAsync<TEntity>(this EntityService<TEntity> entityService, params Guid[] roleIds)
            where TEntity : Entity
        {
            var service =
                entityService.EntityServiceDependency.ServiceProvider
                    .GetRequiredService<IApplicationUserService>();

            return service.IsCurrentUserInRoleAsync(roleIds);
        }
    }

    public interface IApplicationUserService
    {
        Task<ApplicationUserDetailDto> GetCurrentUserAsync();
        Task<bool> IsCurrentUserInRoleAsync(params Guid[] roleIds);
        Task<bool> IsInRoleAsync(Guid userId, params Guid[] roleIds);
    }

    public class ApplicationUserService : EntityService<ApplicationUser>, IApplicationUserService
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

        public async Task<IEnumerable<Guid>> GetRoleIdsAsync(Guid userId)
        {
            var userRoleIds = await _context
                .Set<ApplicationUserRole>()
                    .Where(_ => _.UserId == userId)
                    .Select(_ => _.RoleId)
                    .OrderBy(_ => _)
                    .ToListAsync();

            return userRoleIds;
        }

        public async Task<bool> IsInRoleAsync(Guid userId, params Guid[] roleIds)
        {
            var userRoleIds = await _scopedCache.GetOrCreateAsync(userId.ToString(), async () =>
            {
                return await _context
                    .Set<ApplicationUserRole>()
                        .Where(_ => _.UserId == userId)
                        .Select(_ => _.RoleId)
                        .ToListAsync();
            });

            return roleIds.Any(_ => userRoleIds.Contains(_));
        }

        protected override IQueryable<ApplicationUser> AppendOrderBy(IQueryable<ApplicationUser> query)
        {
            return query.OrderBy(_ => _.Email);
        }
    }
}
