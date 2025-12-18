using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices;

public abstract class TimerHostedService : BaseHostedService, IBackgroundTriggerable
{
    private IBackgroundTrigger _trigger = null;

    protected TimerHostedService(
        IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime appLifetime,
        ILogger<TimerHostedService> logger,
        IApplicationSettings applicationSettings)
        : base(serviceScopeFactory, appLifetime, logger, applicationSettings)
    {
    }

    protected override HostedServicesConfiguration GetConfiguration()
    {
        var config = base.GetConfiguration();
        return config;
    }

    protected override void PostPrepareConfiguration()
    {
        base.PostPrepareConfiguration();

        if (_configuration.ExecuteMode == null || 
            _configuration.ExecuteMode == HostedServicesExecuteModeType.OneTime)
        {
            _configuration.ExecuteMode = HostedServicesExecuteModeType.Interval;
        }

        if (_configuration.ExecuteMode == HostedServicesExecuteModeType.Interval)
        {
            _configuration.Interval ??= TimeSpan.FromMinutes(1);
            _configuration.TriggerExecuteWaitTimeout = null;
            _trigger = null;
        }
        else if (_configuration.ExecuteMode == HostedServicesExecuteModeType.Trigger)
        {
            _configuration.Interval = null;
            _configuration.TriggerExecuteWaitTimeout ??= TimeSpan.FromMinutes(1);

            var serviceTriggerType = typeof(IBackgroundTrigger<>).MakeGenericType(this.GetType());
            _trigger = (IBackgroundTrigger)_serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService(serviceTriggerType);
        }
    }

    protected override bool CanStart()
    {
        if (!base.CanStart())
            return false;

        if (_trigger != null ||
            _configuration.ExecuteMode == HostedServicesExecuteModeType.Trigger ||
            _configuration.TriggerExecuteWaitTimeout.HasValue)
        {
            if (_trigger == null)
            {
                _logger.LogError($"HostedService '{Name}' is configured to use Trigger mode, but no IHostedServiceTrigger is registered.");
                return false;
            }
            if (_configuration.ExecuteMode != HostedServicesExecuteModeType.Trigger)
            {
                _logger.LogError($"HostedService '{Name}' has a Trigger registered, but is not configured to use Trigger mode.");
                return false;
            }
            if (!_configuration.TriggerExecuteWaitTimeout.HasValue)
            {
                _logger.LogError($"HostedService '{Name}' is configured to use Trigger mode, but no TriggerExecuteWaitTimeout is configured.");
                return false;
            }
        }

        if (_configuration.ExecuteMode == HostedServicesExecuteModeType.Interval)
        {
            if (!_configuration.Interval.HasValue)
            {
                _logger.LogError($"HostedService '{Name}' is configured to use Interval mode, but no Interval is configured.");
                return false;
            }
        }

        if (_configuration.ExecuteMode == HostedServicesExecuteModeType.OneTime)
        {
            _logger.LogError($"HostedService '{Name}' is configured to use OneTime mode, but this is not allowed in TimerHostedService.");
            return false;
        }

        return true;
    }

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_configuration.ExecuteMode == HostedServicesExecuteModeType.Trigger)
                {
                    await _trigger.WaitAsync(_configuration.TriggerExecuteWaitTimeout.Value, cancellationToken);
                }

                await base.ExecuteAsync(cancellationToken);

                var delay = await GetWaitIntervalAsync();
                await Task.Delay(delay, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Fatal Error: '{Name}'");
        }
    }

    private async Task<TimeSpan> GetWaitIntervalAsync()
    {
        if (_configuration.ExecuteMode == HostedServicesExecuteModeType.Trigger)
            return TimeSpan.Zero;

        if (BackgroundServiceInfoEnabled == false)
            return _configuration.Interval.Value;

        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<IDbContext>();

        var backgroundServiceInfo = await GetBackgroundServiceInfoAsync(context);

        if (backgroundServiceInfo.NextExecuteAt.HasValue)
        {
            var delay = backgroundServiceInfo.NextExecuteAt.Value - _dateTimeService.Now();

            if (delay < TimeSpan.Zero)
                return TimeSpan.Zero;

            return delay;
        }

        return _configuration.Interval.Value;
    }
}