using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;

using System.Diagnostics;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public class DataSeedSection : Section
    {
        public Type DataSeedType { get; set; }
    }

    public interface IDataSeed : ITypedScopedDependency<IDataSeed>
    {
        decimal Priority { get; }
        bool AutoExecute { get; set; }

        Task SeedAsync(CancellationToken cancellationToken);
    }

    public class DataSeedService : IScopedDependency
    {
        private readonly ILogger<DataSeedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly SectionManager _sectionManager;
        public DataSeedService(ILogger<DataSeedService> logger,
            SectionManager sectionManager,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _sectionManager = sectionManager;
            _serviceProvider = serviceProvider;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var dataSeedGroupedPriority = _serviceProvider.GetServices<IDataSeed>()
                .Where(_ => _.AutoExecute)
                .GroupBy(_ => _.Priority)
                .OrderBy(_ => _.Key)
                .ToList();

            foreach (var dataSeedGroup in dataSeedGroupedPriority)
            {
                var tasks = new List<Task>();

                foreach (var dataSeed in dataSeedGroup)
                {
                    var dataSeedType = dataSeed.GetType();

                    tasks.Add(TaskExtension.StartNewWithCurrentUser(_serviceProvider, async (serviceScopeInner, ct) =>
                    {
                        var serviceProvider = serviceScopeInner.ServiceProvider;

                        var dataSeedService = serviceProvider.GetService<DataSeedService>();
                        var dataSeedInner = serviceProvider.GetService(dataSeedType) as IDataSeed;

                        await dataSeedService.ExecuteSeedAsync(dataSeedInner, ct);
                    }, cancellationToken));
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task ExecuteSeedAsync(IDataSeed dataSeed, CancellationToken cancellationToken)
        {
            using (var section = _sectionManager.CreateSectionScope<DataSeedSection>())
            {
                var dataSeedSection = _serviceProvider.GetService<DataSeedSection>();
                var dataSeedType = dataSeedSection.DataSeedType = dataSeed.GetType();

                try
                {
                    await dataSeed.SeedAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while executing data seed: '{dataSeedType.Name}'");

#if DEBUG
                    Debugger.Break();
#endif
                }
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