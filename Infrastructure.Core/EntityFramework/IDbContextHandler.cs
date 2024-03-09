using Audit.EntityFramework;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SoftwaredeveloperDotAt.Infrastructure.Core.Audit;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChangeTracked;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete;
using System.Linq.Expressions;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public interface IDbContextHandler
    {
        Task UpdateDatabaseAsync<TDbContext>(IHost host)
            where TDbContext : DbContext;
        void DBContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options);

        void OnModelCreating(ModelBuilder modelBuilder);

        void UpdateChangeTrackedEntity(DbContext context);
    }

    public abstract class BaseDbContextHandler : IDbContextHandler, ITypedSingletonDependency<IDbContextHandler>
    {
        public virtual async Task UpdateDatabaseAsync<TDbContext>(IHost host)
            where TDbContext : DbContext
        {
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

                var databaseCreator = context.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;

                if (!await databaseCreator.ExistsAsync())
                {
                    scope.ServiceProvider
                    .GetService<IApplicationSettings>()
                    .HostedServices[
                    nameof(DataSeedHostedService)].Enabled = true;

                    await databaseCreator.CreateAsync();
                }

                await context.Database.MigrateAsync();
            }
        }

        public virtual void DBContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
        {
            options.AddInterceptors(new AuditSaveChangesInterceptor());

            /* 
             * options.AddInterceptors(serviceProvider.GetRequiredService<ChangeTrackedEntitySaveChangesInterceptor>());
            */

            options.UseLazyLoadingProxies();

            //.UseCamelCaseNamingConvention();

            var hostEnvironment = serviceProvider.GetService<IHostEnvironment>();
            if (hostEnvironment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }

            options.ConfigureWarnings(warnings =>
            {
                warnings.Default(WarningBehavior.Ignore);
                warnings.Ignore(RelationalEventId.MultipleCollectionIncludeWarning);
            });

            //options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
        }


        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            ApplyBaseEntity(modelBuilder);

            ApplyEnumToStringValueConverter(modelBuilder);
            ApplyDateTime(modelBuilder);
            ApplyDecimal(modelBuilder);
            ApplyChangeTrackedEntity(modelBuilder);
            ApplyApplicationUser(modelBuilder);
            ApplyAuditEntity(modelBuilder);
            ApplyGlobalFilters(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseSoftwaredeveloperDotAtDbContext).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }

        public abstract void ApplyBaseEntity(ModelBuilder modelBuilder);

        public abstract void ApplyChangeTrackedEntity(ModelBuilder modelBuilder);

        public virtual void ApplyAuditEntity(ModelBuilder modelBuilder)
        {

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(IAudit).IsAssignableFrom(e.ClrType)))
            {
                var tableName = entityType.GetTableName() ?? entityType.Name;
                if (!entityType.Name.EndsWith(AuditExtensions.auditPostfix))
                    tableName = tableName + AuditExtensions.auditPostfix;

                entityType.SetTableName(tableName);
                entityType.SetSchema("audit");
            }
        }

        public virtual void ApplyDateTime(ModelBuilder modelBuilder)
        {

        }

        public virtual void ApplyDecimal(ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties()))
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetPrecision(18);
                    property.SetScale(2);
                }
            }
        }
        public bool UseEnumAsString { get; set; } = false;
        public virtual void ApplyEnumToStringValueConverter(ModelBuilder modelBuilder)
        {
            if (UseEnumAsString == false)
                return;
            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()))
            {
                var enumType = GetEnumType(property.ClrType);
                if (enumType == null)
                    continue;

                var type = typeof(EnumToStringConverter<>).MakeGenericType(enumType);

                var converter = Activator.CreateInstance(type, new ConverterMappingHints()) as ValueConverter;
                property.SetValueConverter(converter);
            }
        }

        private static Type GetEnumType(Type type)
        {
            if (type.IsEnum)
                return type;

            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            if (nullableUnderlyingType?.IsEnum ?? false)
                return nullableUnderlyingType;

            return null;
        }

        public void UpdateChangeTrackedEntity(DbContext context)
        {
            var currentUserService = context.GetService<ICurrentUserService>();

            //var currentUserService = ServiceProvider.GetService<ICurrentUserService>();

            DateTime utcNow = DateTime.UtcNow;

            var changedEntries = context.ChangeTracker
                .Entries()
                .Where(e => e.Entity is ChangeTrackedEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified))
                .ToList();

            var deletedEntries = context.ChangeTracker
                .Entries()
                .Where(e => e.Entity is ChangeTrackedEntity && e.State == EntityState.Deleted)
                .ToList();

            var now = DateTime.Now;

            foreach (var entityEntry in changedEntries)
            {
                var currentUserId = currentUserService?.GetCurrentUserId();
                if (currentUserId == null)
                    throw new InvalidOperationException("CurrentUser not set.");

                var baseEntity = (ChangeTrackedEntity)entityEntry.Entity;

                baseEntity.DateModified = now;
                baseEntity.ModifiedById = currentUserId.Value;

                if (entityEntry.State == EntityState.Added)
                {
                    baseEntity.DateCreated = now;
                    baseEntity.CreatedById = currentUserId.Value;
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    var originalDateCreated = entityEntry.OriginalValues.GetValue<DateTime>(nameof(ChangeTrackedEntity.DateCreated));
                    var originalCreatedById = entityEntry.OriginalValues.GetValue<Guid>(nameof(ChangeTrackedEntity.CreatedById));

                    baseEntity.DateCreated = originalDateCreated;
                    baseEntity.CreatedById = originalCreatedById;
                }
            }
        }

        public abstract void ApplyApplicationUser(ModelBuilder modelBuilder);


        private void ApplyGlobalFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                this.GetType()
                    .GetMethod(nameof(ConfigureGlobalFilters),
                            BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, new object[] { modelBuilder, entityType, null });
            }
        }

        protected void ConfigureGlobalFilters<TEntity>(ModelBuilder modelBuilder, IMutableEntityType entityType, IMutableNavigation mutableNavigation = null)
            where TEntity : class
        {
            var shoudlFilterEntity = (bool)this.GetType()
                .GetMethod(nameof(ShouldFilterEntity),
                                   BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(mutableNavigation?.ClrType ?? entityType.ClrType)
                .Invoke(this, null);

            if (entityType.BaseType == null && shoudlFilterEntity)
            {
                var filterExpression = CreateFilterExpression<TEntity>(mutableNavigation);
                if (filterExpression != null)
                {
                    modelBuilder.Entity<TEntity>()
                        .HasQueryFilter(filterExpression);
                }
            }


            if (mutableNavigation == null)
            {
                foreach (var navigation in entityType.GetNavigations().ToList())
                {
                    if (!navigation.IsOnDependent || navigation.IsCollection)
                        continue;

                    if (typeof(IEntity).IsAssignableFrom(navigation.ClrType))
                    {
                        this.GetType()
                           .GetMethod(nameof(ConfigureGlobalFilters), BindingFlags.Instance | BindingFlags.NonPublic)
                           .MakeGenericMethod(entityType.ClrType)
                           .Invoke(this, new object[] {
                     modelBuilder,
                       entityType,
                       navigation
                           });
                    }
                }
            }
        }

        protected virtual bool ShouldFilterEntity<TEntity>()
            where TEntity : class
        {
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                return true;
            }

            return false;
        }

        public bool IsSoftDeleteFilterEnabled { get; set; } = true;

        protected virtual Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>(IMutableNavigation mutableNavigation = null)
            where TEntity : class
        {
            if (typeof(ISoftDelete).IsAssignableFrom(mutableNavigation?.ClrType ?? typeof(TEntity)))
            {
                if (mutableNavigation != null)
                {
                    Expression<Func<TEntity, bool>> softDeleteFilter =
                        e => !IsSoftDeleteFilterEnabled || !EF.Property<ISoftDelete>(e, mutableNavigation.Name).IsDeleted;

                    return softDeleteFilter;
                }
                else
                {
                    Expression<Func<TEntity, bool>> softDeleteFilter =
                        e => !IsSoftDeleteFilterEnabled || !((ISoftDelete)e).IsDeleted;

                    return softDeleteFilter;
                }
            }


            /*
             * 
             *  modelBuilder.Entity<EventCategory>()
                    .HasQueryFilter(t => !t.Event.IsDeleted);*/
            return null;
        }
    }
}
