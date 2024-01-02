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

        protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await DoTimeWorkInternalAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while executing background hosted service: '{GetType().FullName}'");
                }

                await Task.Delay(TimeSpan.FromSeconds(_hostedServicesConfiguration.IntervalInSeconds), cancellationToken);
            }
        }

        protected abstract Task DoTimeWorkInternalAsync(CancellationToken cancellationToken);
    }
}
