using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

using ExtendableEnums.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SoftwaredeveloperDotAt.Infrastructure.Core.Audit;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChangeTracked;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportDefault;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

public abstract class BaseDbContextHandler : IDbContextHandler
{
    private readonly ILogger<BaseDbContextHandler> _logger;
    private readonly IEnumerable<Type> _backgroundTriggerableTypes;

    protected BaseDbContextHandler(ILogger<BaseDbContextHandler> logger)
    {
        _logger = logger;
        _backgroundTriggerableTypes = AssemblyUtils.GetDerivedTypes(typeof(IBackgroundTriggerable<>));
    }

    public virtual async Task UpdateDatabaseAsync(DbContext context)
    {
        var databaseCreator = context.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;

        var dbExists = false;

        try
        {
            dbExists = await databaseCreator.ExistsAsync();
        }
        catch (DbException ex) when (ex.SqlState == "3D000") // '3D000: Datenbank XXX existiert nicht'

        {
            dbExists = false;
        }

        if (!dbExists)
        {
            await databaseCreator.CreateAsync();
        }

        await context.Database.MigrateAsync();
    }

    protected string GetConnectionString(IServiceProvider serviceProvider, string connectionStringKey)
    {
        var connectionString = serviceProvider.GetService<IApplicationSettings>().ConnectionStrings[connectionStringKey];

        return connectionString;
    }

    public virtual void DBContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options, string connectionStringKey = "DbContextConnection")
    {
        options.UseLazyLoadingProxies();

        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        options.UseLoggerFactory(loggerFactory);

        var hostEnvironment = serviceProvider.GetService<IHostEnvironment>();
        if (hostEnvironment.IsDevelopment())
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        }

        options.ConfigureWarnings(warnings =>
        {
            warnings.Ignore(RelationalEventId.MultipleCollectionIncludeWarning);
        });
    }

    public virtual void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.ApplyExtendableEnumConversions();
        ApplyBaseEntity(modelBuilder);
        ApplyEnumToStringValueConverter(modelBuilder);
        ApplyDateTime(modelBuilder);
        ApplyDecimal(modelBuilder);
        ApplyChangeTrackedEntity(modelBuilder);
        ApplyApplicationUser(modelBuilder);
        ApplyAuditEntity(modelBuilder);
        ApplyGlobalFilters(modelBuilder);
        ApplyIndex(modelBuilder);
        ApplyAutoInclude(modelBuilder);
        ApplyRemoveForeignKeyAttribute(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SoftwaredeveloperDotAtDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(context.GetType().Assembly);
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

    public virtual void ApplyAutoInclude(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IMultiLingualEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Navigation(nameof(IMultiLingualEntity.Translations)).AutoInclude();
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
        var transactionService = context.GetService<DbContextTransaction>();
        var transactionDateTime = transactionService.TransactionTime.Value;

        var entityEntries = context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IAuditableEntity)
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted)
            .ToList();

        if (!entityEntries.Any())
            return;

        var transactionId = Guid.NewGuid();
        var idbContext = context.As<IDbContext>();

        foreach (var entityEntry in entityEntries)
        {
            var entity = entityEntry.Entity;
            var auditableEntity = entity.To<IAuditableEntity>();

            if (auditableEntity == null)
                continue;

            //if(entityEntry.State == EntityState.Modified)
            //{
            //    var changes = new List<string>();
            //    foreach (var property in entityEntry.OriginalValues.Properties)
            //    {
            //        var original = entityEntry.OriginalValues[property];
            //        var current = entityEntry.CurrentValues[property];

            //        if (!object.Equals(original, current))
            //        {
            //            changes.Add($"Property: {property}, Original value: {original}, New value: {current}");
            //        }
            //    }
            //}

            auditableEntity
                .CreateEntityAudit(idbContext, entityEntry.GetAuditActionType(), transactionDateTime, transactionId);
        }
    }

    public void HandleChangeTrackedEntity(DbContext context)
    {
        var currentUserService = context.GetService<ICurrentUserService>();

        var transactionService = context.GetService<DbContextTransaction>();
        var transactionDateTime = transactionService.TransactionTime.Value;

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

        foreach (var entityEntry in changedEntries)
        {
            var currentUserId = currentUserService?.GetCurrentUserId();
            if (currentUserId == null)
                throw new InvalidOperationException("CurrentUser not set.");

            var baseEntity = (ChangeTrackedEntity)entityEntry.Entity;

            baseEntity.DateModified = transactionDateTime;
            baseEntity.ModifiedById = currentUserId.Value;

            if (entityEntry.State == EntityState.Added)
            {
                baseEntity.DateCreated = transactionDateTime;
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

    public virtual void TriggerBackground(DbContext context)
    {
        var backgroundTriggerQueue = context.GetService<BackgroundTriggerQueue>();

        while (backgroundTriggerQueue.TryDequeue(out var trigger))
        {
            trigger.Trigger();
        }
    }

    public virtual void EnqueueBackgroundTrigger(DbContext context)
    {
        var backgroundTriggerQueue = context.GetService<BackgroundTriggerQueue>();

        foreach (var backgroundTriggerableType in _backgroundTriggerableTypes)
        {
            var entityType = backgroundTriggerableType.GetTriggerableEntityType();

            var isEntityAdded = context.ChangeTracker
                .Entries()
                .Any(e => e.Entity.GetType() == entityType &&
                          e.State == EntityState.Added);

            if (isEntityAdded)
            {
                var triggerType = typeof(IBackgroundTrigger<>).MakeGenericType(backgroundTriggerableType);
                var backgroundTrigger = (IBackgroundTrigger)context.GetService(triggerType);
                backgroundTriggerQueue.Enqueue(backgroundTrigger);
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