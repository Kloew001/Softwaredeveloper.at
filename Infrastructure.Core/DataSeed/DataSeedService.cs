using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.DataSeed;

public class DataSeedSection : Section
{
    public Type DataSeedType { get; set; }
}

[ScopedDependency]
public class DataSeedService
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
        var watch = Stopwatch.StartNew();

        var dataSeedGroupedPriority = _serviceProvider.GetServices<IDataSeed>()
            .Where(_ => _.AutoExecute)
            .GroupBy(_ => _.Priority)
            .Select(_ => new
            {
                Key = _.Key,
                DataSeedTypes = _.Select(__ => __.GetType())
            })
            .OrderBy(_ => _.Key)
            .ToList();

        foreach (var dataSeedGroup in dataSeedGroupedPriority)
        {
            await Parallel.ForEachAsync(dataSeedGroup.DataSeedTypes,
              new ParallelOptions
              {
                  MaxDegreeOfParallelism = Environment.ProcessorCount,
                  CancellationToken = cancellationToken
              },
              async (dataSeedType, ct) =>
              {
                  using (var childScope = _serviceProvider.CreateChildScope())
                  {
                      var dataSeedService = childScope.ServiceProvider.GetService<DataSeedService>();
                      var dataSeedInner = childScope.ServiceProvider.GetService(dataSeedType) as IDataSeed;

                      await dataSeedService.ExecuteSeedAsync(dataSeedInner, cancellationToken);

                      Debug.WriteLine($"Data seed '{dataSeedType.Name}' executed.");
                  }
              });
        }

        var time = watch.ElapsedMilliseconds;
    }

    private async Task ExecuteSeedAsync(IDataSeed dataSeed, CancellationToken cancellationToken)
    {
        using (var section = _sectionManager.CreateSectionScope<DataSeedSection>())
        {
            var currentUserService = _serviceProvider.GetService<ICurrentUserService>();
            currentUserService.SetCurrentUserId(ApplicationUserIds.ServiceAdminId);

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