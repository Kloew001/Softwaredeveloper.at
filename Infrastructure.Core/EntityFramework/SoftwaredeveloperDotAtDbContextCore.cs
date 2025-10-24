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
    public SoftwaredeveloperDotAtDbContextCore(DbContextOptions options)
        : base(options)
    {
    }

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