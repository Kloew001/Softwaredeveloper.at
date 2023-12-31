using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public abstract class TimerHostedService : IHostedService, IDisposable
    {
        protected Timer _timer;
        protected readonly ILogger _logger;
        protected readonly IApplicationSettings _applicationSettings;
        protected readonly HostedServicesConfiguration _hostedServicesConfiguration;
        protected readonly IHostApplicationLifetime _appLifetime;
        private readonly CancellationTokenSource _appStoppingTokenSource = new CancellationTokenSource();

        protected TimerHostedService(IHostApplicationLifetime appLifetime,
            ILogger logger,
           IApplicationSettings applicationSettings)
        {
            _logger = logger;
            _applicationSettings = applicationSettings;

            _hostedServicesConfiguration = _applicationSettings.HostedServicesConfiguration[GetType().Name];
            _appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"IHostedService.StartAsync for {GetType().Name}");

            _appLifetime.ApplicationStarted.Register(
                async () =>
                    await ExecuteAsync(_appStoppingTokenSource.Token).ConfigureAwait(false)
            );

            return Task.CompletedTask;
        }

        protected void DoWork(object state)
        {
            try
            {
                DoWorkInternal(state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while executing background hosted service: '{GetType().FullName}'");
            }
        }

        protected abstract void DoWorkInternal(object state);

        protected Task ExecuteAsync(CancellationToken appStoppingToken)
        {
            _logger.LogInformation($"IHostedService.ExecuteAsync for {GetType().Name}");

            if (_hostedServicesConfiguration.Enabled == true)
            {
                _timer = new Timer(DoWork, null,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(_hostedServicesConfiguration.IntervalInSeconds));
            }

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"IHostedService.StopAsync for {GetType().Name}");

            _appStoppingTokenSource.Cancel();

            Dispose();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();
        }
    }
}
