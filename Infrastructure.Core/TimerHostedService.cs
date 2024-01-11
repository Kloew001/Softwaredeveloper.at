using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public abstract class TimerHostedService : BaseHostedSerivice
    {
        protected TimerHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IHostApplicationLifetime appLifetime,
            ILogger<BaseHostedSerivice> logger,
            IApplicationSettings applicationSettings)
            : base(serviceScopeFactory, appLifetime, logger, applicationSettings)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (_hostedServicesConfiguration == null ||
                _hostedServicesConfiguration?.Enabled != true)
            {
                _logger.LogWarning($"IHostedService {Name} do not have configuration");
                return;
            }

            _logger.LogInformation($"IHostedService execute for {Name}");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await CreateOrUpdateBackgroundServiceInfo();

                    await ExecuteInternalAsync(cancellationToken);

                    await FinsihedBackgroundServiceInfo();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while executing background hosted service: '{Name}'");

                    await ErrorBackgroundServiceInfo(ex);
                }

                await Task.Delay(TimeSpan.FromSeconds(_hostedServicesConfiguration.IntervalInSeconds), cancellationToken);
            }
        }
    }
}
