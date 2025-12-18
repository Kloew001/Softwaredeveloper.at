using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual;

/* 
HOW TO USE:

public class MultilingualDataSeedHostedService : BaseMultilingualDataSeedHostedService
{
    public MultilingualDataSeedHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<BaseMultilingualDataSeedHostedService> logger, IApplicationSettings applicationSettings, IHostApplicationLifetime appLifetime) : base(serviceScopeFactory, logger, applicationSettings, appLifetime)
    {
    }

    protected override string GetFileName()
    {
        return $"{Path.GetDirectoryName(typeof(MultilingualDataSeedHostedService).Assembly.Location)}\\Sections\\MultilingualSection\\Content\\{"Multilingual.json"}";
    }
}
 */

public abstract class BaseMultilingualDataSeedHostedService : TimerHostedService
{
    public BaseMultilingualDataSeedHostedService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BaseMultilingualDataSeedHostedService> logger,
        IApplicationSettings applicationSettings,
        IHostApplicationLifetime appLifetime)
        : base(serviceScopeFactory, appLifetime, logger, applicationSettings)
    {
        BackgroundServiceInfoEnabled = false;

        var filePath = GetFileName();

        _watcher = new FileSystemWatcher(System.IO.Path.GetDirectoryName(filePath));

        _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
        _watcher.Filter = "*.json";
        _watcher.EnableRaisingEvents = true;

        _watcher.Changed += (sender, e) =>
        {
            _reloadJson = true;
        };
    }

    protected override HostedServicesConfiguration GetConfiguration()
    {
        var config = base.GetConfiguration();

        config.ExecuteMode ??= HostedServicesExecuteModeType.Interval;

        if(config.ExecuteMode == HostedServicesExecuteModeType.Interval)
            config.Interval ??= TimeSpan.FromSeconds(1);

        return config;
    }

    private FileSystemWatcher _watcher;
    private bool _reloadJson = true;

    //return $"{Path.GetDirectoryName(typeof(MultilingualDataSeedHostedService).Assembly.Location)}\\Sections\\MultilingualSection\\Content\\{"Multilingual.json"}";
    protected abstract string GetFileName();

    protected override async Task ExecuteInternalAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        if (_reloadJson == false)
            return;

        var filePath = GetFileName();

        var multilingualService = scope.ServiceProvider.GetService<JsonMultilingualService>();

        await multilingualService.ImportAsync(FileUtiltiy.GetContent(filePath));

        _reloadJson = false;
    }
}