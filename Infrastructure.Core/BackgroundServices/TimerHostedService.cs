﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        protected override bool CanStart()
        {
            if(!base.CanStart())
                return false;

            if (_hostedServicesConfiguration?.Interval.HasValue == false)
            {
                _logger.LogWarning($"IHostedService {Name} do not have Interval configuration");
                return false;
            }

            return true;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await base.ExecuteAsync(cancellationToken);

                    await Task.Delay(_hostedServicesConfiguration.Interval.Value, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatal Error: '{Name}'");
            }
        }
    }
}
