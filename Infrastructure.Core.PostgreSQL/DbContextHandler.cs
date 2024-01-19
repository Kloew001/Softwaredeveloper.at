using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public class PostgreSQLDbContextHandler : BaseDbContextHandler
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
                    .HostedServicesConfiguration[
                    nameof(DataSeedHostedService)].Enabled = true;

                    databaseCreator.Create();
                }

                await context.Database.MigrateAsync();
            }
        }

        public override void ApplyChangeTrackedEntity(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(ChangeTrackedEntity).IsAssignableFrom(e.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ChangeTrackedEntity.DateCreated))
                    .HasDefaultValueSql("NOW()");
                //.ValueGeneratedOnAdd();
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ChangeTrackedEntity.DateModified))
                    .HasDefaultValueSql("NOW()");
                //.ValueGeneratedOnAddOrUpdate();

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
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToLocalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToLocalTime() : null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified) : null);

            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()))
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                    property.SetColumnType("TIMESTAMP WITHOUT TIME ZONE");
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                    property.SetColumnType("TIMESTAMP WITHOUT TIME ZONE");
                }
                else if (property.ClrType == typeof(TimeSpan) || property.ClrType == typeof(TimeSpan?))
                {
                    property.SetColumnType("bigint");
                }
            }
        }

        public override void ApplyApplicationUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUserRole>(entity =>
            {
                entity.ToTable("ApplicationUserRole", "identity");

                entity.HasKey(_ => new { _.UserId, _.RoleId });

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
}