using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public interface IDbContextHandler
    {
        Task UpdateDatabaseAsync<TDbContext>(IHost host)
            where TDbContext : DbContext;

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

        public abstract void ApplyChangeTrackedEntity(ModelBuilder modelBuilder);
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
