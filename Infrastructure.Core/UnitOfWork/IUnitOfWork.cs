using Microsoft.Extensions.DependencyInjection;

using System;
using System.Runtime.CompilerServices;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.UnitOfWork
{
    public static class UnitOfWorkExtensions
    {
        public static void AddUnitOfWork(this IServiceCollection services)
        {
            services.AddTransient<IUnitOfWorkScope, UnitOfWorkScope>();
            services.AddScoped<UnitOfWork>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetService<UnitOfWork>());
            services.AddScoped<ICurrentUnitOfWorkScope>(sp => sp.GetService<UnitOfWork>());
        }
    }

    public interface IUnitOfWork
    {
        IUnitOfWorkScope CreateScope();
        Task SaveAsync(CancellationToken cancellationToken = default);
    }

    public interface ICurrentUnitOfWorkScope
    {
        IUnitOfWorkScope CurrentWorkScope { get; }
    }

    public interface IUnitOfWorkScope : IDisposable
    {
        Guid Id { get; }
        internal IDbContext Context { get; }
        internal bool IsDisposed { get; }
    }

    public class UnitOfWorkScope : IUnitOfWorkScope, IDisposable
    {
        private readonly IServiceScope _serviceScope;
        
        public Guid Id { get; set; }
        public IDbContext Context { get; private set; }
        public bool IsDisposed { get; private set; }
        
        public UnitOfWorkScope(IServiceScopeFactory serviceScopeFactory)
        {
            Id = Guid.NewGuid();
            _serviceScope = serviceScopeFactory.CreateScope();
            Context = _serviceScope.ServiceProvider.GetService<IDbContext>();
        }

        public void Dispose()
        {
            _serviceScope.Dispose();

            IsDisposed = true;
        }
    }

    public class UnitOfWork : IUnitOfWork, ICurrentUnitOfWorkScope
    {
        private readonly IServiceProvider _serviceProvider;

        public IUnitOfWorkScope CurrentWorkScope => Scopes.Where(_ => !_.IsDisposed).Last();
        public IDbContext CurrentContext => CurrentWorkScope.Context;

        public List<IUnitOfWorkScope> Scopes { get; private set; } = [];

        public UnitOfWork(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            CreateScope();
        }

        public IUnitOfWorkScope CreateScope()
        {
            var workScope = _serviceProvider.GetService<IUnitOfWorkScope>();
            Scopes.Add(workScope);
            return workScope;
        }

        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return CurrentContext.SaveChangesAsync(cancellationToken);
        }

    }
}
