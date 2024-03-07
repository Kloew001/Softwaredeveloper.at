using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using System.Net.Http.Headers;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity
{
    public abstract class IdentitySoftwaredeveloperDotAtDbContext :
        IdentityDbContext<
            ApplicationUser,
            ApplicationRole,
            Guid,
            IdentityUserClaim<Guid>,
            ApplicationUserRole,
            IdentityUserLogin<Guid>,
            IdentityRoleClaim<Guid>,
            IdentityUserToken<Guid>>
    {
        public IdentitySoftwaredeveloperDotAtDbContext()
        {
        }

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

            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("ApplicationUserClaim", "identity");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("ApplicationUserLogin", "identity");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("ApplicationRoleClaim", "identity");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("ApplicationUserToken", "identity");
        }
    }

}
