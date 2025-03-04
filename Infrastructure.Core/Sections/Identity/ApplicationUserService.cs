using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.Audit;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;

public static class IApplicationUserServiceExtensions
{
    public static ValueTask<bool> IsCurrentUserInRoleAsync<TEntity>(this EntityService<TEntity> entityService, params Guid[] roleIds)
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
    Task<Guid> CreateRoleAsync(Guid id, string roleName);
    Task<Guid> CreateUserAsync(CreateApplicationUserIdentity identity, CancellationToken ct = default);
    Task DeleteUserAsync(Guid userId);
}

public interface IApplicationUserService
{
    Task<ApplicationUserDetailDto> GetCurrentUserAsync();
    ValueTask<bool> IsCurrentUserInRoleAsync(params Guid[] roleIds);
    ValueTask<bool> IsInRoleAsync(Guid userId, params Guid[] roleIds);
    void RemoveRoleCache();
    Task<ApplicationUser> GetUserByIdAsync(Guid id);
    Task<ApplicationUser> GetUserByEMailAsync(string email);
    Task<ApplicationUser> GetUserByUserNameAsync(string username);
    Task<ApplicationUser> CreateUserAsync(CreateApplicationUserIdentity identity, CancellationToken ct = default);
    Task<Guid> CreateRoleAsync(Guid id, string roleName);
    Task SetPreferedCultureAsync(string cultureName);
}

[ScopedDependency<IApplicationUserService>]
public class ApplicationUserService : EntityService<ApplicationUser>, IApplicationUserService
{
    public ApplicationUserService(EntityServiceDependency<ApplicationUser> entityServiceDependency)
        : base(entityServiceDependency)
    {
    }

    public virtual Task<ApplicationUser> GetUserByIdAsync(Guid id)
    {
        return GetSingleAsync((query) => query.Where(_ => _.Id == id));
    }

    public virtual Task<ApplicationUser> GetUserByEMailAsync(string email)
    {
        email = email.ToUpper().Trim();
        return GetSingleAsync((query) => query.Where(_ => _.NormalizedEmail == email));
    }

    public virtual Task<ApplicationUser> GetUserByUserNameAsync(string username)
    {
        username = username.ToUpper().Trim();

        return GetSingleAsync((query) => query.Where(_ => _.NormalizedUserName == username));
    }

    public Task<ApplicationUserDetailDto> GetCurrentUserAsync()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();

        if (currentUserId == null)
            return null;

        return GetSingleByIdAsync<ApplicationUserDetailDto>(currentUserId.Value);
    }

    public Task<bool> IsEMailAlreadyInUse(string email)
    {
        var normalizedEmail = email.ToUpper().Trim();
        return _context
            .Set<ApplicationUser>()
                .Where(_ => _.NormalizedEmail == normalizedEmail)
                .AnyAsync();
    }

    public Task<bool> IsUserNameAlreadyInUse(string username)
    {
        var normalizedUserName = username.ToUpper().Trim();
        return _context
            .Set<ApplicationUser>()
                .Where(_ => _.NormalizedUserName == normalizedUserName)
                .AnyAsync();
    }

    public Task<bool> IsAnyUserNameAlreadyInUse(IEnumerable<string> usernames)
    {
        var normalizedUserNames = usernames
            .Select(_ => _.ToUpper().Trim())
            .ToList();

        return _context
            .Set<ApplicationUser>()
                .Where(_ => normalizedUserNames.Contains(_.NormalizedUserName))
                .AnyAsync();
    }

    public ValueTask<bool> IsCurrentUserInRoleAsync(params Guid[] roleIds)
    {
        return IsInRoleAsync(_currentUserService.GetCurrentUserId().Value, roleIds);
    }

    public const string _getRoleIdsCacheKey = $"{nameof(ApplicationUserService)}_{nameof(GetRoleIdsAsync)}_";

    public void RemoveRoleCache()
    {
        _cacheService.MemoryCache.RemoveStartsWith(_getRoleIdsCacheKey);
    }

    public async ValueTask<IEnumerable<Guid>> GetRoleIdsAsync(Guid userId)
    {
        var userRoleIds = await _cacheService.MemoryCache.GetOrCreateAsync(_getRoleIdsCacheKey + userId, async (entry) =>
        {
            return await _context
                .Set<ApplicationUserRole>()
                    .Where(_ => _.UserId == userId)
                    .Select(_ => _.RoleId)
                    .OrderBy(_ => _)
                    .ToListAsync();
        });

        return userRoleIds;
    }

    public virtual async ValueTask<bool> IsInRoleAsync(Guid userId, params Guid[] roleIds)
    {
        var userRoleIds = await GetRoleIdsAsync(userId);

        return roleIds.Any(_ => userRoleIds.Contains(_));
    }

    protected override IQueryable<ApplicationUser> AppendOrderBy(IQueryable<ApplicationUser> query)
    {
        return query.OrderBy(_ => _.Email);
    }

    public virtual async Task<Guid> CreateRoleAsync(Guid id, string roleName)
    {
        var applicationUserIdentityService = EntityServiceDependency.ServiceProvider.GetRequiredService<IApplicationUserIdentityService>();

        id = await applicationUserIdentityService
            .CreateRoleAsync(id, roleName);

        return id;
    }

    public virtual async Task<ApplicationUser> CreateUserAsync(CreateApplicationUserIdentity identity, CancellationToken ct = default)
    {
        var applicationUserIdentityService = EntityServiceDependency.ServiceProvider.GetRequiredService<IApplicationUserIdentityService>();

        var currentUserId = _currentUserService.GetCurrentUserId();

        if (!currentUserId.HasValue)
            _currentUserService.SetCurrentUserId(ApplicationUserIds.ServiceAdminId);

        var applicationUserId = await applicationUserIdentityService.CreateUserAsync(identity, ct);

        try
        {
            var applicationUser = await GetUserByIdAsync(applicationUserId);

            if (await _accessService.EvaluateAsync(applicationUser, (accessCondition, securityEntity) =>
                        accessCondition.CanCreateAsync(securityEntity)) == false)
                throw new UnauthorizedAccessException();

            using (var childScope = EntityServiceDependency.ServiceProvider.CreateChildScope())
            {
                var context = childScope.ServiceProvider.GetRequiredService<IDbContext>();
                var applicationUserService = childScope.ServiceProvider.GetRequiredService<IApplicationUserService>();

                var applicationUserScoped = await applicationUserService.GetUserByIdAsync(applicationUserId);

                applicationUserScoped.CreateEntityAudit(context, AuditActionType.Created, DateTime.Now, Guid.NewGuid());

                await context.SaveChangesAsync();
            }

            applicationUser.Reload();

            return applicationUser;
        }
        catch
        {
            await DeleteIdentityAsync(applicationUserId);
            throw;
        }
        finally
        {
            if (!currentUserId.HasValue)
                _currentUserService.SetCurrentUserId(null);
        }
    }

    public virtual async Task DeleteIdentityAsync(Guid id)
    {
        var applicationUserIdentityService = EntityServiceDependency.ServiceProvider.GetRequiredService<IApplicationUserIdentityService>();

        await applicationUserIdentityService
            .DeleteUserAsync(id);
    }

    public async Task SetPreferedCultureAsync(string cultureName)
    {
        var currentUserId = _currentUserService.GetCurrentUserId().Value;

        var applicationUser = await GetSingleByIdAsync(currentUserId);

        if (cultureName.IsNullOrEmpty())
        {
            applicationUser.PreferedCulture = null;
        }
        else
        {
            applicationUser.PreferedCulture =
                await _context.Set<MultilingualCulture>()
                    .Where(_ => _.IsActive &&
                                _.Name == cultureName.ToLower())
                    .SingleAsync();
        }

        await UpdateAsync(applicationUser);

        await SaveAsync(applicationUser);

        EntityServiceDependency
            .ServiceProvider
            .GetService<ICurrentLanguageService>()
            .As<CurrentLanguageService>()
            ?.RemoveCache(currentUserId);
    }
}
