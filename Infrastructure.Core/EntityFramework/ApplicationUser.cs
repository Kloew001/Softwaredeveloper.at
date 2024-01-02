using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

public static class ApplicationUserIds
{
    public static Guid ServiceAdminId = Guid.Parse("01706405-553A-48F9-BF78-C86F88AA6096");
    public static Guid AdminId = Guid.Parse("244D7C67-37AC-448C-92C4-398BC9090C71");
}

public class ApplicationUser : BaseEntity
{
    public string UserName { get; set; }

    public string NormalizedUserName { get; set; }

    public string Email { get; set; }

    public string NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PasswordHash { get; set; }

    public string SecurityStamp { get; set; }

    public string ConcurrencyStamp { get; set; }

    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public virtual ICollection<ApplicationUserClaim> ApplicationUserClaims { get; set; } = new List<ApplicationUserClaim>();

    public virtual ICollection<ApplicationUserLogin> ApplicationUserLogins { get; set; } = new List<ApplicationUserLogin>();

    public virtual ICollection<ApplicationUserToken> ApplicationUserTokens { get; set; } = new List<ApplicationUserToken>();

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}

public class ApplicationUserClaim : BaseEntity
{
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

public class ApplicationRole : BaseEntity
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

public class ApplicationRoleClaim : BaseEntity
{
    public Guid RoleId { get; set; }

    public string ClaimType { get; set; }

    public string ClaimValue { get; set; }

    public virtual ApplicationRole Role { get; set; }
}

public static class ApplicationUserExtensions
{
    public static void AppApplicationUser(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUserRole>(entity =>
        {
            entity.ToTable("ApplicationUserRole", "identity");

            entity.HasKey(_=> new { _.UserId, _.RoleId });

            entity.HasIndex(new[] { "RoleId" }, "IX_ApplicationUserRole_RoleId");

            entity.HasOne<ApplicationRole>()
                    .WithMany()
                    .HasForeignKey("RoleId");

            entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey("UserId");
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("ApplicationUser", "identity");

            entity.HasKey(x => x.Id);

            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        modelBuilder.Entity<ApplicationUserClaim>(entity =>
        {
            entity.ToTable("ApplicationUserClaim", "identity");

            entity.HasIndex(e => e.UserId, "IX_ApplicationUserClaim_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.ApplicationUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<ApplicationUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.ToTable("ApplicationUserLogin", "identity");

            entity.HasIndex(e => e.UserId, "IX_ApplicationUserLogin_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.ApplicationUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<ApplicationUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.ToTable("ApplicationUserToken", "identity");

            entity.HasOne(d => d.User).WithMany(p => p.ApplicationUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("ApplicationRole", "identity");

            entity.HasKey(x => x.Id);

            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<ApplicationRoleClaim>(entity =>
        {
            entity.ToTable("ApplicationRoleClaim", "identity");

            entity.HasIndex(e => e.RoleId, "IX_ApplicationRoleClaim_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.ApplicationRoleClaims).HasForeignKey(d => d.RoleId);
        });
    }

}