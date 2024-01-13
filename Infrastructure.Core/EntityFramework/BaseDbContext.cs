using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public abstract class BaseDbContext : DbContext,
        IDbContext,
        ITypedScopedService<IDbContext>

    {
        protected BaseDbContext()
        {
        }

        protected BaseDbContext(DbContextOptions options) : base(options)
        {
        }

        //protected IServiceProvider ServiceProvider { get; }

        //public BaseDbContext(IServiceProvider serviceProvider)
        //{
        //    ServiceProvider = serviceProvider;
        //}

        //public BaseDbContext(DbContextOptions options, IServiceProvider serviceProvider)
        //    : base(options)
        //{
        //    ServiceProvider = serviceProvider;
        //}

        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public virtual DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }

        public virtual DbSet<ApplicationUserLogin> ApplicationUserLogins { get; set; }

        public virtual DbSet<ApplicationUserToken> ApplicationUserTokens { get; set; }

        public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }

        public virtual DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }

        public virtual DbSet<ApplicationRoleClaim> ApplicationRoleClaims { get; set; }

        public virtual DbSet<EmailMessage> EmailMessages { get; set; }

        public virtual DbSet<BackgroundserviceInfo> BackgroundserviceInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyEnumToStringValueConverter();
            modelBuilder.ApplyDateTime();
            modelBuilder.ApplyChangeTrackedEntity();

            modelBuilder.AppApplicationUser();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDbContext).Assembly);
        }

        public override int SaveChanges()
        {
            BeforeSaveChanges();

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            BeforeSaveChanges();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void BeforeSaveChanges()
        {
            this.UpdateChangeTrackedEntity();
        }

        TEntity IDbContext.CreateProxy<TEntity>(params object[] constructorArguments) where TEntity : class
        {
            return ProxiesExtensions.CreateProxy<TEntity>(this, constructorArguments);
        }

        public async Task UpdateDatabaseAsync()
        {
            var databaseCreator = this.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
            if (!databaseCreator.Exists())
                databaseCreator.Create();

            await this.Database.MigrateAsync();
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