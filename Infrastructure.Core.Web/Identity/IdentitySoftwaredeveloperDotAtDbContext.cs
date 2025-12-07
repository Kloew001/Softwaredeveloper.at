using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;

public abstract class IdentitySoftwaredeveloperDotAtDbContext :
    IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        ApplicationUserClaim,
        ApplicationUserRole,
        ApplicationUserLogin,
        ApplicationRoleClaim,
        ApplicationUserToken>
{

    public IdentitySoftwaredeveloperDotAtDbContext(DbContextOptions options)
    : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dbContextHandler = this.GetService<EntityFramework.IDbContextHandler>() as EntityFramework.BaseDbContextHandler;

        dbContextHandler.ApplyDateTime(modelBuilder);
        //dbContextHandler.ApplyEnumToStringValueConverter(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentitySoftwaredeveloperDotAtDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

        //modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        //modelBuilder.ApplyConfiguration(new ApplicationRoleConfiguration());
        //modelBuilder.ApplyConfiguration(new ApplicationUserRoleConfiguration());

        modelBuilder.Entity<ApplicationUserClaim>().ToTable("ApplicationUserClaim", "identity");
        modelBuilder.Entity<ApplicationUserLogin>().ToTable("ApplicationUserLogin", "identity");
        modelBuilder.Entity<ApplicationUserToken>().ToTable("ApplicationUserToken", "identity");
    }
}