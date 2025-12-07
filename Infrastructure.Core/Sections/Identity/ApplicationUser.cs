
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SoftwaredeveloperDotAt.Infrastructure.Core.Audit;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;

public static class ApplicationUserIds
{
    public static Guid ServiceAdminId = Guid.Empty;
}

public static class ICurrentUserServiceExtension
{
    public static bool IsServiceAdmin(this ICurrentUserService currentUserService)
    {
        return currentUserService.GetCurrentUserId() == ApplicationUserIds.ServiceAdminId;
    }
}

public class ApplicationUser : Entity, IAuditableEntity<ApplicationUserAudit>
{
    public bool IsEnabled { get; set; }

    public string UserName { get; set; }

    public string NormalizedUserName { get; set; }

    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PasswordHash { get; set; }

    public string SecurityStamp { get; set; }

    public string ConcurrencyStamp { get; set; }

    public Guid? PreferedCultureId { get; set; }
    public virtual MultilingualCulture PreferedCulture { get; set; }

    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public DateTime DateCreated { get; set; }

    public virtual ICollection<ApplicationUserClaim> ApplicationUserClaims { get; set; } = new List<ApplicationUserClaim>();

    public virtual ICollection<ApplicationUserLogin> ApplicationUserLogins { get; set; } = new List<ApplicationUserLogin>();

    public virtual ICollection<ApplicationUserToken> ApplicationUserTokens { get; set; } = new List<ApplicationUserToken>();

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

    public virtual ICollection<ApplicationUserAudit> Audits { get; set; }
}

public class ApplicationUserAudit : EntityAudit<ApplicationUser>
{
    public bool IsEnabled { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class ApplicationUserClaim
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string ClaimType { get; set; }

    public string ClaimValue { get; set; }

    public virtual ApplicationUser User { get; set; }
}

public class ApplicationUserLogin
{
    public string LoginProvider { get; set; }

    public string ProviderKey { get; set; }

    public string ProviderDisplayName { get; set; }

    public Guid UserId { get; set; }

    public virtual ApplicationUser User { get; set; }
}

public class ApplicationUserToken
{
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; }

    public string LoginProvider { get; set; }

    public string Name { get; set; }

    public string Value { get; set; }
}

public class ApplicationRole : Entity
{
    public string Name { get; set; }

    public string NormalizedName { get; set; }

    public string ConcurrencyStamp { get; set; }

    public virtual ICollection<ApplicationRoleClaim> ApplicationRoleClaims { get; set; } = new List<ApplicationRoleClaim>();

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}

public class ApplicationUserRole
{
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; }

    public Guid RoleId { get; set; }
    public virtual ApplicationRole Role { get; set; }
}

public class ApplicationRoleClaim : Entity
{
    public Guid RoleId { get; set; }

    public string ClaimType { get; set; }

    public string ClaimValue { get; set; }

    public virtual ApplicationRole Role { get; set; }
}