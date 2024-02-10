using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices
{
    public abstract class TimerHostedService : BaseHostedService
    {
        protected TimerHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IHostApplicationLifetime appLifetime,
            ILogger<BaseHostedService> logger,
            IApplicationSettings applicationSettings)
            : base(serviceScopeFactory, appLifetime, logger, applicationSettings)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await base.ExecuteAsync(cancellationToken);

                    await Task.Delay(TimeSpan.FromSeconds(_hostedServicesConfiguration.IntervalInSeconds), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatal Error: '{Name}'");
            }
        }
    }
}
