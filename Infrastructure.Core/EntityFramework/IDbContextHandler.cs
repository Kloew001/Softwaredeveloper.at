using DocumentFormat.OpenXml.Vml.Office;

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
using SoftwaredeveloperDotAt.Infrastructure.Core.DataSeed;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChangeTracked;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SoftDelete;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportDefault;

using System.Linq.Expressions;
using System.Reflection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public interface IDbContextHandler
    {
        Task UpdateDatabaseAsync(DbContext context);

        void DBContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options);

        void OnModelCreating(ModelBuilder modelBuilder);

        void HandleChangeTrackedEntity(DbContext context);
        void HandleEntityAudit(DbContext context);
    }

    public abstract class BaseDbContextHandler : IDbContextHandler, ITypedSingletonDependency<IDbContextHandler>
    {
        public virtual async Task UpdateDatabaseAsync(DbContext context)
        {
            var databaseCreator = context.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;

            if (!await databaseCreator.ExistsAsync())
            {
                //scope.ServiceProvider
                //.GetService<IApplicationSettings>()
                //.HostedServices[
                //nameof(DataSeedHostedService)].Enabled = true;

                await databaseCreator.CreateAsync();
            }

            await context.Database.MigrateAsync();
        }

        public virtual void DBContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
        {
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
            ApplyIndex(modelBuilder);
            ApplyRemoveForeignKeyAttribute(modelBuilder);


            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SoftwaredeveloperDotAtDbContext).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }

        private void ApplyRemoveForeignKeyAttribute(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var foreignKey in entityType.GetForeignKeys().ToList())
                {
                    var principalType = foreignKey.PrincipalEntityType.ClrType;
                    var declaringType = foreignKey.DeclaringEntityType.ClrType;

                    if (declaringType.GetCustomAttribute<VirtualRelationAttribute>() != null ||
                        principalType.GetCustomAttribute<VirtualRelationAttribute>() != null)
                    {
                        entityType.RemoveForeignKey(foreignKey);
                    }
                }
            }
        }

        public virtual void ApplyIndex(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IReferencedToEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(
                            nameof(IReferencedToEntity.ReferenceId),
                            nameof(IReferencedToEntity.ReferenceType));
                }

                if (typeof(IEntityTranslation).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(nameof(IEntityTranslation.CoreId), nameof(IEntityTranslation.CultureId))
                        .IsUnique();
                }
                if (typeof(IEntityAudit).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(nameof(IEntityAudit.AuditId));

                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(nameof(IEntityAudit.TransactionId));
                }

                if (typeof(ISupportDefault).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(nameof(ISupportDefault.IsDefault))
                        .IsUnique();
                }
            }
        }

        public abstract void ApplyBaseEntity(ModelBuilder modelBuilder);

        public abstract void ApplyChangeTrackedEntity(ModelBuilder modelBuilder);

        public virtual void ApplyAuditEntity(ModelBuilder modelBuilder)
        {
            const string auditPostfix = "Audit";

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(IEntityAudit).IsAssignableFrom(e.ClrType)))
            {
                var tableName = entityType.GetTableName() ?? entityType.Name;

                if (!entityType.Name.EndsWith(auditPostfix))
                    tableName = tableName + auditPostfix;

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


        public void HandleEntityAudit(DbContext context)
        {
            var entityEntries = context.ChangeTracker
                .Entries()
                .Where(e => e.Entity is IAuditableEntity)
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            if (!entityEntries.Any())
                return;

            var now = DateTime.Now;
            var transactionId = Guid.NewGuid();

            foreach (var entityEntry in entityEntries)
            {
                var entity = entityEntry.Entity;
                var auditableEntity = entity.To<IAuditableEntity>();

                if (auditableEntity == null)
                    continue;

                if(entityEntry.State == EntityState.Modified)
                {
                    var changes = new List<string>();
                    foreach (var property in entityEntry.OriginalValues.Properties)
                    {
                        var original = entityEntry.OriginalValues[property];
                        var current = entityEntry.CurrentValues[property];

                        if (!object.Equals(original, current))
                        {
                            changes.Add($"Property: {property}, Original value: {original}, New value: {current}");
                        }
                    }
                }

                var entityAuditType = auditableEntity.GetEntityAuditType();

                var entityAudit = context.As<IDbContext>()
                        .GetType()
                        .GetMethod(nameof(IDbContext.CreateEntity))
                        .MakeGenericMethod(entityAuditType)
                        .Invoke(context, null)
                        .As<IEntityAudit>();

                auditableEntity.CopyPropertiesTo(entityAudit);

                entityAudit.Id = Guid.NewGuid();

                entityAudit.AuditId = auditableEntity.Id;
                entityAudit.Audit = auditableEntity;

                entityAudit.AuditDate = now;
                entityAudit.AuditAction = entityEntry.State.ToString();

                //var entityFrameworkEvent = auditEvent?.GetEntityFrameworkEvent();
                entityAudit.TransactionId = transactionId.ToString();

                //entityAudit.CallingMethod = auditEvent.Environment?.CallingMethodName;
                //entityAudit.MachineName = auditEvent.Environment?.MachineName;
            }
        }

        public void HandleChangeTrackedEntity(DbContext context)
        {
            var currentUserService = context.GetService<ICurrentUserService>();

            var changedEntries = context.ChangeTracker
                .Entries()
                .Where(e => e.Entity is ChangeTrackedEntity)
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified)
                .ToList();

            //var deletedEntries = context.ChangeTracker
            //    .Entries()
            //    .Where(e => e.Entity is ChangeTrackedEntity && e.State == EntityState.Deleted)
            //    .ToList();

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

    [AttributeUsage(AttributeTargets.Property)]
    public class VirtualRelationAttribute : Attribute
    {
    }
}
