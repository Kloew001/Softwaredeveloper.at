﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

[ScopedDependency<IDbContext>]
public abstract class SoftwaredeveloperDotAtDbContext : DbContext, IDbContext
{
    public bool UseProxy { get; set; } = true;

    public SoftwaredeveloperDotAtDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

    public virtual DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }

    public virtual DbSet<ApplicationUserLogin> ApplicationUserLogins { get; set; }

    public virtual DbSet<ApplicationUserToken> ApplicationUserTokens { get; set; }

    public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }

    public virtual DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }

    public virtual DbSet<ApplicationRoleClaim> ApplicationRoleClaims { get; set; }

    public virtual DbSet<ChronologyEntry> ChronologyEntries { get; set; }

    public virtual DbSet<BinaryContent> Contents { get; set; }

    public virtual DbSet<EmailMessage> EmailMessages { get; set; }

    public virtual DbSet<AsyncTaskOperation> AsyncTaskOperations { get; set; }

    public virtual DbSet<BackgroundserviceInfo> BackgroundserviceInfos { get; set; }

    public virtual DbSet<MultilingualCulture> MultilingualCultures { get; set; }
    public virtual DbSet<MultilingualGlobalText> MultilingualGlobalTexts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dbContextHandler = this.GetService<IDbContextHandler>();
        dbContextHandler.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    public override int SaveChanges()
    {
        BeforeSaveChanges();

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        BeforeSaveChanges();

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    [ScopedDependency]
    public class DbContextTransaction
    {
        public Guid TransactionId { get; set; }
        public DateTime? TransactionTime { get; set; }

        private readonly IDateTimeService _dateTimeService;
        public DbContextTransaction(IDateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService;
        }

        public void EnsureTime()
        {
            if (TransactionTime == null)
                TransactionTime = _dateTimeService.Now();
        }
    }

    private void BeforeSaveChanges()
    {
        var dbContextHandler = this.GetService<IDbContextHandler>();

        if (dbContextHandler == null)
            throw new Exception($"Could not resolve {nameof(IDbContextHandler)}");

        var transactionService = this.GetService<DbContextTransaction>();
        transactionService.EnsureTime();

        var transactionDateTime = transactionService.TransactionTime.Value;

        dbContextHandler.HandleEntityAudit(this, transactionDateTime);
        dbContextHandler.HandleChangeTrackedEntity(this, transactionDateTime);
    }

    public TEntity CreateEntity<TEntity>()
        where TEntity : class
    {
        return CreateEntityAync<TEntity>().GetAwaiter().GetResult();
    }

    public async Task<TEntity> CreateEntityAync<TEntity>()
        where TEntity : class
    {
        if (UseProxy)
        {
            var proxy = CreateProxy<TEntity>();
            await AddAsync(proxy);
            return proxy;
        }
        else
        {
            //var instance = Activator.CreateInstance(typeof(TEntity));
            var constructor = typeof(TEntity).GetConstructor(Type.EmptyTypes);
            var entity = (TEntity)constructor.Invoke(new object[] { });

            await AddAsync(entity);
            return entity;
        }
    }

    public TEntity CreateProxy<TEntity>(params object[] constructorArguments)
        where TEntity : class
    {
        return ProxiesExtensions.CreateProxy<TEntity>(this, constructorArguments);
    }
}