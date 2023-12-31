using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public interface IDbContext
    {
        DatabaseFacade Database { get; }

        DbSet<TEntity> Set<TEntity>()
            where TEntity : class;

        EntityEntry<TEntity> Add<TEntity>(TEntity entity)
            where TEntity : class;
        
        ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
                where TEntity : class;
        void AddRange(IEnumerable<object> entities);
        Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);

        EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
            where TEntity : class;

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public abstract class BaseDbContext : DbContext,
        IDbContext, 
        IApplicationIdentitDbContext, 
        IEmailMessageDbContext,
        ITypedScopedService<IDbContext>,
        ITypedScopedService<IApplicationIdentitDbContext>,
        ITypedScopedService<IEmailMessageDbContext>

    {
        private readonly IServiceProvider _serviceProvider;

        public BaseDbContext(IServiceProvider provider)
        {
            _serviceProvider = provider;
        }

        public BaseDbContext(DbContextOptions options, IServiceProvider provider)
            : base(options)
        {
            _serviceProvider = provider;
        }

        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public virtual DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }

        public virtual DbSet<ApplicationUserLogin> ApplicationUserLogins { get; set; }

        public virtual DbSet<ApplicationUserToken> ApplicationUserTokens { get; set; }

        public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }

        public virtual DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }

        public virtual DbSet<ApplicationRoleClaim> ApplicationRoleClaims { get; set; }

        public virtual DbSet<EmailMessage> EmailMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyEnumToStringValueConverter();
            modelBuilder.ApplyDateTime();

            ApplyChangeTrackedEntity(modelBuilder);

            modelBuilder.AppApplicationUser();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDbContext).Assembly);
        }

        public override int SaveChanges()
        {
            BeforeSaveChanges();

            return base.SaveChanges();
            //int result = 0;

            //result = base.SaveChanges();

            //ChangeTracker.Clear();

            //return result;
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            BeforeSaveChanges();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void BeforeSaveChanges()
        {
            var currentUserService = _serviceProvider.GetRequiredService<ICurrentUserService>();

            var changedEntries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is ChangeTrackedEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified))
                .ToList();

            var deletedEntries = ChangeTracker
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

        private void ApplyChangeTrackedEntity(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(ChangeTrackedEntity).IsAssignableFrom(e.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ChangeTrackedEntity.DateCreated))
                    .HasDefaultValueSql("CURRENT_DATE ")
                    .ValueGeneratedOnAdd();
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ChangeTrackedEntity.DateModified))
                    .HasDefaultValueSql("CURRENT_DATE")
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

    }

    public static class DbContextExension
    {
        public static void ApplyEnumToStringValueConverter(this ModelBuilder modelBuilder)
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

        public static void ApplyDateTime(this ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()))
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("TIMESTAMP WITHOUT TIME ZONE");
                }
                else if (property.ClrType == typeof(TimeSpan) || property.ClrType == typeof(TimeSpan?))
                {
                    property.SetColumnType("bigint");
                }
            }
        }

        private static Type? GetEnumType(Type type)
        {
            if (type.IsEnum)
                return type;

            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            if (nullableUnderlyingType?.IsEnum ?? false)
                return nullableUnderlyingType;

            return null;
        }
    }
}
