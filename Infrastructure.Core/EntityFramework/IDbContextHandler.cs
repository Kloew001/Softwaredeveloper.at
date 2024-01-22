using Audit.EntityFramework;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SoftwaredeveloperDotAt.Infrastructure.Core.Audit;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public interface IDbContextHandler
    {
        Task UpdateDatabaseAsync<TDbContext>(IHost host)
            where TDbContext : DbContext;
        void DBContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options);

        void OnModelCreating(ModelBuilder modelBuilder);
        void ApplyDateTime(ModelBuilder modelBuilder);
        void ApplyEnumToStringValueConverter(ModelBuilder modelBuilder);
        void ApplyChangeTrackedEntity(ModelBuilder modelBuilder);
        void ApplyApplicationUser(ModelBuilder modelBuilder);

        void UpdateChangeTrackedEntity(DbContext context);
    }

    public abstract class BaseDbContextHandler : IDbContextHandler, ITypedSingletonService<IDbContextHandler>
    {
        public abstract Task UpdateDatabaseAsync<TDbContext>(IHost host)
            where TDbContext : DbContext;

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
            ApplyEnumToStringValueConverter(modelBuilder);
            ApplyDateTime(modelBuilder);
            ApplyChangeTrackedEntity(modelBuilder);
            ApplyApplicationUser(modelBuilder);
            ApplyAuditEntity(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDbContext).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }

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

        public abstract void ApplyDateTime(ModelBuilder modelBuilder);

        public virtual void ApplyEnumToStringValueConverter(ModelBuilder modelBuilder)
        {
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
    }
}
