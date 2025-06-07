using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

public class ScopedDbContextFactory<TContext> :
    IDbContextFactory<TContext>,
    ITypedScopedDependency<IDbContextFactory<TContext>>
    where TContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScopedDbContextFactory(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public TContext CreateDbContext()
    {
        var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TContext>();
    }
}