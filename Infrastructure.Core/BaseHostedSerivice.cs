using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public abstract class BaseHostedSerivice : IHostedService, IDisposable
    {
        protected readonly IServiceScopeFactory _serviceScopeFactory;
        protected readonly ILogger<BaseHostedSerivice> _logger;
        protected readonly IApplicationSettings _applicationSettings;
        protected readonly HostedServicesConfiguration _hostedServicesConfiguration;
        protected readonly IHostApplicationLifetime _appLifetime;
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        protected BaseHostedSerivice(
            IServiceScopeFactory serviceScopeFactory,
            IHostApplicationLifetime appLifetime,
            ILogger<BaseHostedSerivice> logger,
            IApplicationSettings applicationSettings)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _appLifetime = appLifetime;
            _logger = logger;
            _applicationSettings = applicationSettings;
            _applicationSettings.HostedServicesConfiguration.TryGetValue(GetType().Name, out _hostedServicesConfiguration);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"IHostedService.StartAsync for {GetType().Name}");

            _appLifetime.ApplicationStarted.Register(async () => await ExecuteAsync(cancellationToken));

            return Task.CompletedTask;
        }

        protected async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"IHostedService.ExecuteAsync for {GetType().Name}");

            if (_hostedServicesConfiguration == null ||
                _hostedServicesConfiguration?.Enabled == true)
            {
                await ExecuteInternalAsync(cancellationToken);
            }
        }

        protected abstract Task ExecuteInternalAsync(CancellationToken cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"IHostedService.StopAsync for {GetType().Name}");

            _cancellationToken.Cancel();

            Dispose();

            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
        }

    }
}
