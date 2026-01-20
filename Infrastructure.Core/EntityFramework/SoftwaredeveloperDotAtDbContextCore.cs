using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

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

public abstract class SoftwaredeveloperDotAtDbContextCore : DbContext, IDbContext
{
    public bool UseProxy { get; set; } = true;

    protected IDbContextHandler _dbContextHandler;

    public SoftwaredeveloperDotAtDbContextCore(DbContextOptions options)
        : base(options)
    {
        _dbContextHandler = this.GetService<IDbContextHandler>();

        if (_dbContextHandler == null)
            throw new Exception($"Could not resolve {nameof(IDbContextHandler)}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dbContextHandler = this.GetService<IDbContextHandler>();
        dbContextHandler.OnModelCreating(modelBuilder, this);
    }

    public override int SaveChanges()
    {
        BeforeSaveChanges();

        var result = base.SaveChanges();

        AfterSaveChanges();

        return result;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        BeforeSaveChanges();

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        AfterSaveChanges();

        return result;
    }

    protected virtual void BeforeSaveChanges()
    {
        var transactionService = this.GetService<DbContextTransaction>();
        transactionService.EnsureTime();

        _dbContextHandler.HandleEntityAudit(this);
        _dbContextHandler.HandleChangeTrackedEntity(this);
        _dbContextHandler.EnqueueBackgroundTrigger(this);
    }

    protected virtual void AfterSaveChanges()
    {
        _dbContextHandler.TriggerBackground(this);
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