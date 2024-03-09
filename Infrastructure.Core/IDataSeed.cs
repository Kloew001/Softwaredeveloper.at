using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface IDataSeed : ITypedScopedDependency<IDataSeed>
    {
        decimal Priority { get; }
        bool ExecuteInThread { get; set; }
        bool AutoExecute { get; set; }

        Task SeedAsync(CancellationToken cancellationToken);
    }

    public class DataSeedService : IScopedDependency
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
            var dataSeedGrouped = _serviceProvider.GetServices<IDataSeed>()
                .Where(_ => _.AutoExecute)
                .GroupBy(_ => _.Priority)
                .OrderBy(_ => _.Key)
                .ToList();

            foreach (var dataSeedGroup in dataSeedGrouped)
            {
                var tasks = new List<Task>();

                foreach (var dataSeed in dataSeedGroup.Where(_ => _.ExecuteInThread == true))
                {
                    var dataSeedType = dataSeed.GetType();

                    tasks.Add(TaskExtension.StartNewWithCurrentUser(_serviceProvider, cancellationToken, async (serviceScopeInner, ct) =>
                    {
                        var serviceProvider = serviceScopeInner.ServiceProvider;

                        var dataSeedService = serviceProvider.GetService<DataSeedService>();
                        var dataSeedInner = serviceProvider.GetService(dataSeedType) as IDataSeed;

                        await dataSeedService.ExecuteSeedAsync(dataSeedInner, ct);
                    }));
                }

                foreach (var dataSeed in dataSeedGroup.Where(_ => _.ExecuteInThread == false))
                {
                    await ExecuteSeedAsync(dataSeed, cancellationToken);
                }

                Task.WaitAll(tasks.ToArray(), cancellationToken);
            }
        }

        private async Task ExecuteSeedAsync(IDataSeed dataSeed, CancellationToken cancellationToken)
        {
            var dataSeedType = dataSeed.GetType();

            try
            {
                await dataSeed.SeedAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while executing data seed: '{dataSeedType.Name}'");
            }
        }
    }

    public class DataSeedHostedService : BaseHostedService
    {
        public DataSeedHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IHostApplicationLifetime appLifetime,
            ILogger<BaseHostedService> logger,
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