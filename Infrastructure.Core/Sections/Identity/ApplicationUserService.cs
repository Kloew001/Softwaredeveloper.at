

using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
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

    public class CreateApplicationUserIdentity
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool EmailConfirmed { get; set; }
        public string[] RoleNames { get; set; }
    }

    public interface IApplicationUserIdentityService
    {
        Task<Guid> CreateAsync(CreateApplicationUserIdentity identity, CancellationToken ct = default);
        Task DeleteAsync(Guid userId);
    }

    public interface IApplicationUserService
    {
        Task<ApplicationUserDetailDto> GetCurrentUserAsync();
        Task<bool> IsCurrentUserInRoleAsync(params Guid[] roleIds);
        Task<bool> IsInRoleAsync(Guid userId, params Guid[] roleIds);
        Task<ApplicationUser> CreateIdentityInternalAsync(CreateApplicationUserIdentity identity, CancellationToken ct = default);
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

        public Task<bool> IsEMailAlreadyInUse(string email)
        {
            return  _context
                .Set<ApplicationUser>()
                    .Where(_ => _.NormalizedEmail == email.ToUpper())
                    .AnyAsync();
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

        public virtual async Task<ApplicationUser> CreateIdentityInternalAsync(CreateApplicationUserIdentity identity, CancellationToken ct = default)
        {
            var applicationUserIdentityService = EntityServiceDependency.ServiceProvider.GetRequiredService<IApplicationUserIdentityService>();
            
            var applicationUserId = await applicationUserIdentityService
                .CreateAsync(identity, ct);

            try
            {
                var applicationUser = await GetSingleByIdInternalAsync(applicationUserId);

                var accessConditionInfo = ResolveAccessConditionInfo(applicationUser);
                var accessCondition = accessConditionInfo.AccessCondition;

                if (await accessCondition.CanCreateAsync(accessConditionInfo.SecurityEntity) == false)
                    throw new UnauthorizedAccessException();

                return applicationUser;
            }
            catch
            {
                await DeleteIdentityAsync(applicationUserId);
                throw;
            }
        }

        public virtual async Task DeleteIdentityAsync(Guid id)
        {
            var applicationUserIdentityService = EntityServiceDependency.ServiceProvider.GetRequiredService<IApplicationUserIdentityService>();

            await applicationUserIdentityService
                .DeleteAsync(id);
        }
    }
}
