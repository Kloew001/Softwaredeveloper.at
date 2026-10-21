using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public class DistributedLockCleanupHostedService : TimerHostedService
{
    private static readonly TimeSpan RetentionBuffer = TimeSpan.FromMinutes(15);

    public DistributedLockCleanupHostedService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DistributedLockCleanupHostedService> logger,
        IApplicationSettings applicationSettings,
        IHostApplicationLifetime appLifetime)
        : base(serviceScopeFactory, appLifetime, logger, applicationSettings)
    {
    }

    protected override HostedServicesConfiguration GetConfiguration()
    {
        var config = base.GetConfiguration();

        config.ExecuteMode ??= HostedServicesExecuteModeType.Interval;

        if (config.ExecuteMode == HostedServicesExecuteModeType.Interval)
            config.Interval ??= TimeSpan.FromMinutes(15);

        return config;
    }

    protected override async Task ExecuteInternalAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var context = scope.ServiceProvider.GetRequiredService<IDbContext>();
        var dateTimeService = scope.ServiceProvider.GetRequiredService<IDateTimeService>();

        var threshold = dateTimeService.Now().Subtract(RetentionBuffer);

        var deleted = await context.Set<DistributedLock>()
            .Where(_ => _.ExpiresAt < threshold)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
            _logger.LogInformation("Removed {Count} expired distributed lock lease(s).", deleted);
    }
}
