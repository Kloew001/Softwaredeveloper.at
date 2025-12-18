using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices;

namespace SampleApp.Application.Sections.PersonSection;

public class PersonHostedService : TimerHostedService
{
    public PersonHostedService(IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime appLifetime,
        ILogger<PersonHostedService> logger,
        IApplicationSettings applicationSettings)
        : base(serviceScopeFactory, appLifetime, logger, applicationSettings)
    {
    }

    protected override HostedServicesConfiguration GetConfiguration()
    {
        var config = base.GetConfiguration();
        config.Interval ??= TimeSpan.FromSeconds(5);
        return config;
    }

    protected override Task ExecuteInternalAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
