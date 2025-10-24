using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

[ScopedDependency<IDbContext>]
public abstract class SoftwaredeveloperDotAtDbContext : SoftwaredeveloperDotAtDbContextCore
{
    public SoftwaredeveloperDotAtDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

    public virtual DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }

    public virtual DbSet<ApplicationUserLogin> ApplicationUserLogins { get; set; }

    public virtual DbSet<ApplicationUserToken> ApplicationUserTokens { get; set; }

    public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }

    public virtual DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }

    public virtual DbSet<ApplicationRoleClaim> ApplicationRoleClaims { get; set; }

    public virtual DbSet<ChronologyEntry> ChronologyEntries { get; set; }

    public virtual DbSet<BinaryContent> Contents { get; set; }

    public virtual DbSet<EmailMessage> EmailMessages { get; set; }

    public virtual DbSet<AsyncTaskOperation> AsyncTaskOperations { get; set; }

    public virtual DbSet<BackgroundserviceInfo> BackgroundserviceInfos { get; set; }

    public virtual DbSet<MultilingualCulture> MultilingualCultures { get; set; }
    public virtual DbSet<MultilingualGlobalText> MultilingualGlobalTexts { get; set; }
}
