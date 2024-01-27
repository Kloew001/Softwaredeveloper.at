using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoftwaredeveloperDotAt.Infrastructure.Core;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface IDataSeed : ITypedScopedService<IDataSeed>
    {
        int Priority { get; }
        bool ExecuteInThread { get; set; }
        bool AutoExecute { get; set; }

        Task SeedAsync();
    }

    public class DataSeedService : IScopedService
    {
        private readonly ILogger<DataSeedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        public DataSeedService(ILogger<DataSeedService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var dataSeeds = _serviceProvider.GetServices<IDataSeed>()
                .Where(_ => _.AutoExecute)
                .OrderBy(x => x.Priority)
                .ToList();

            var tasks = new List<Task>();

            foreach (var dataSeed in dataSeeds)
            {
                if (dataSeed.ExecuteInThread)
                {
                    var dataSeedType = dataSeed.GetType();

                    tasks.Add(TaskExtension.StartNewWithCurrentUser(_serviceProvider, async (serviceScopeInner) =>
                    {
                        var serviceProvider = serviceScopeInner.ServiceProvider;

                        var dataSeedService = serviceProvider.GetService<DataSeedService>();
                        var dataSeedInner = serviceProvider.GetService(dataSeedType) as IDataSeed;

                        await dataSeedService.ExecuteSeedAsync(dataSeedInner);
                    }));
                }
                else
                {
                    await ExecuteSeedAsync(dataSeed);
                }
            }

            Task.WaitAll(tasks.ToArray(), cancellationToken);
        }

        private async Task ExecuteSeedAsync(IDataSeed dataSeed)
        {
            var dataSeedType = dataSeed.GetType();

            try
            {
                await dataSeed.SeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while executing data seed: '{dataSeedType.Name}'");
            }
        }
    }

    public class DataSeedHostedService : BaseHostedSerivice
    {
        public DataSeedHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IHostApplicationLifetime appLifetime,
            ILogger<BaseHostedSerivice> logger,
            IApplicationSettings applicationSettings)
            : base(serviceScopeFactory, appLifetime, logger, applicationSettings)
        {
        }

        protected override async Task ExecuteInternalAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var currentUserService = scope.ServiceProvider.GetService<ICurrentUserService>();
            currentUserService.SetCurrentUserId(ApplicationUserIds.ServiceAdminId);

            var dataSeedService = scope.ServiceProvider.GetService<DataSeedService>();
            await dataSeedService.ExecuteAsync(cancellationToken);
        }
    }
}