using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public interface IDbContext
    {
        DatabaseFacade Database { get; }

        DbSet<TEntity> Set<TEntity>()
            where TEntity : class;

        TEntity CreateProxy<TEntity>(params object[] constructorArguments)
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
        ITypedScopedService<IDbContext>

    {
        protected IServiceProvider ServiceProvider { get; }

        public BaseDbContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public BaseDbContext(DbContextOptions options, IServiceProvider serviceProvider)
            : base(options)
        {
            ServiceProvider = serviceProvider;
        }

        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public virtual DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }

        public virtual DbSet<ApplicationUserLogin> ApplicationUserLogins { get; set; }

        public virtual DbSet<ApplicationUserToken> ApplicationUserTokens { get; set; }

        public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }

        public virtual DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }

        public virtual DbSet<ApplicationRoleClaim> ApplicationRoleClaims { get; set; }

        public virtual DbSet<EmailMessage> EmailMessages { get; set; }

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
            var c = this.GetService<ICurrentUserService>();

            c = ServiceProvider.GetService<ICurrentUserService>();

            this.UpdateChangeTrackedEntity(c);
        }

        TEntity IDbContext.CreateProxy<TEntity>(params object[] constructorArguments) where TEntity : class
        {
            return ProxiesExtensions.CreateProxy<TEntity>(this, constructorArguments);
        }

        private void ApplyChangeTrackedEntity(ModelBuilder modelBuilder)
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

        public async Task UpdateDatabaseAsync()
        {

            var databaseCreator = this.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
            if (!databaseCreator.Exists())
                databaseCreator.Create();

            await this.Database.MigrateAsync();
        }
    }

    public static class DbContextExension
    {
        public static void AddDbContext<TDbContext>(this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment)
            where TDbContext : BaseDbContext
        {
            //_services.AddScoped<ContextScopedFactory>();
            //_services.AddPooledDbContextFactory<Context>((serviceProvider, optionsBuilder) =>
            //    {
            //        DbContextOptions(optionsBuilder);
            //    });
            //_services.AddScoped<Context>(sp => sp.GetRequiredService<ContextScopedFactory>().CreateDbContext());
            //_services.AddScoped<IDbContext>(sp => sp.GetRequiredService<Context>());

            services.AddDbContext<TDbContext>((sp, o) => o.DbContextOptions(sp, configuration, hostEnvironment));
        }

        public static async Task UpdateDatabaseAsync<TDbContext>(this IHost host)
            where TDbContext : BaseDbContext
        {
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
                await context.UpdateDatabaseAsync();
            }
        }

        public static void DbContextOptions(this DbContextOptionsBuilder options,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment)
        {
            var connectionString = configuration.GetConnectionString("DbContextConnection");

            options.UseNpgsql(connectionString);
            options.UseLazyLoadingProxies();

            /* 
             * options.AddInterceptors(serviceProvider.GetRequiredService<ChangeTrackedEntitySaveChangesInterceptor>());
            */

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

        private static Type GetEnumType(Type type)
        {
            if (type.IsEnum)
                return type;

            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            if (nullableUnderlyingType?.IsEnum ?? false)
                return nullableUnderlyingType;

            return null;
        }

        public static void UpdateChangeTrackedEntity(this DbContext context, ICurrentUserService currentUserService)
        {
            DateTime utcNow = DateTime.UtcNow;

            var changedEntries = context.ChangeTracker
                .Entries()
                .Where(e => e.Entity is ChangeTrackedEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified))
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
    }

    //public class ChangeTrackedEntitySaveChangesInterceptor : SaveChangesInterceptor, IScopedService
    //{
    //    private readonly ICurrentUserService _currentUserService;
    //    public ChangeTrackedEntitySaveChangesInterceptor(ICurrentUserService currentUserService)
    //    {
    //        _currentUserService = currentUserService;
    //    }

    //    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
    //        DbContextEventData eventData,
    //        InterceptionResult<int> result,
    //        CancellationToken cancellationToken = default)
    //    {
    //        if (eventData.Context is not null)
    //        {
    //            UpdateChangeTrackedEntity(eventData.Context);
    //        }

    //        return base.SavingChangesAsync(eventData, result, cancellationToken);
    //    }

    //}
}