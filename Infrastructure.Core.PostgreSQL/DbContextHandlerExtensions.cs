using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public static class DbContextHandlerExtensions
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

            services.AddDbContext<TDbContext>((sp, options) =>
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
            });
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
}