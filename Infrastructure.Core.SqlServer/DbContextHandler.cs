using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public class SqlServerDbContextHandler : BaseDbContextHandler
    {
        public override async Task UpdateDatabaseAsync<TDbContext>(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

                var databaseCreator = context.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;

                if (!databaseCreator.Exists())
                {
                    scope.ServiceProvider
                    .GetService<IApplicationSettings>()
                    .HostedServices[
                    nameof(DataSeedHostedService)].Enabled = true;

                    databaseCreator.Create();
                }

                await context.Database.MigrateAsync();
            }
        }

        public override void DBContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
        {
            var connectionString = serviceProvider.GetService<IApplicationSettings>().ConnectionStrings["DbContextConnection"];

            options.UseSqlServer(connectionString, options =>
            {

            });

            base.DBContextOptions(serviceProvider, options);
        }

        public override void ApplyChangeTrackedEntity(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(ChangeTrackedEntity).IsAssignableFrom(e.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ChangeTrackedEntity.DateCreated))
                    .HasDefaultValueSql("getdate()")
                    .ValueGeneratedOnAdd();
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ChangeTrackedEntity.DateModified))
                    .HasDefaultValueSql("getdate()")
                    .ValueGeneratedOnAddOrUpdate();

                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(nameof(ChangeTrackedEntity.CreatedBy))
                    .WithMany()
                    .OnDelete(DeleteBehavior.NoAction);

                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(nameof(ChangeTrackedEntity.ModifiedBy))
                    .WithMany()
                    .OnDelete(DeleteBehavior.NoAction);
            }
        }

        public override void ApplyDateTime(ModelBuilder modelBuilder)
        {
        }

        public override void ApplyApplicationUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUserRole>(entity =>
            {
                entity.ToTable("ApplicationUserRole", "identity");

                entity.HasKey(_ => new { _.UserId, _.RoleId });

                entity.HasIndex(new[] { "RoleId" }, "IX_ApplicationUserRole_RoleId");

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                entity.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
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
}