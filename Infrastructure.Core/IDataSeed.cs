using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using System;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface IDataSeed : ITypedScopedService<IDataSeed>
    {
        int Priority { get; }
        bool ExecuteInThread { get; set; }
        bool AutoExecute { get; set; }

        Task SeedAsync();
    }

    public class DataSeedHostedService : BaseHostedSerivice
    {
        public DataSeedHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IHostApplicationLifetime appLifetime,
            ILogger<BaseHostedSerivice> logger, IApplicationSettings applicationSettings)
            : base(serviceScopeFactory, appLifetime, logger, applicationSettings)
        {
        }

        protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var currentUserService = scope.ServiceProvider.GetService<ICurrentUserService>();
                currentUserService.SetCurrentUserId(ApplicationUserIds.ServiceAdminId);

                var dataSeeds = scope.ServiceProvider.GetServices<IDataSeed>()
                    .Where(_ => _.AutoExecute)
                    .OrderBy(x => x.Priority)
                    .ToList();

                var tasks = new List<Task>();

                foreach (var dataSeed in dataSeeds)
                {
                    if (dataSeed.ExecuteInThread)
                    {
                        var dataSeedType = dataSeed.GetType();

                        tasks.Add(TaskExtension.StartNewWithCurrentUser(scope.ServiceProvider, async (serviceScopeInner) =>
                        {
                            var dataSeedInner = serviceScopeInner.ServiceProvider.GetService(dataSeedType) as IDataSeed;
                            await dataSeedInner.SeedAsync();
                        }));
                    }
                    else
                    {
                        await dataSeed.SeedAsync();
                    }
                }

                Task.WaitAll(tasks.ToArray(), cancellationToken);
            }
        }
    }
}
