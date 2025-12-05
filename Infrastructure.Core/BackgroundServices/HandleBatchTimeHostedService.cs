using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices;

public abstract class HandleBatchTimeHostedService : TimerHostedService
{
    public HandleBatchTimeHostedService(
        IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime appLifetime,
        ILogger<HandleBatchTimeHostedService> logger,
        IApplicationSettings settings)
        : base(serviceScopeFactory, appLifetime, logger, settings)
    {
    }

    protected override async Task ExecuteInternalAsync(IServiceScope scope, CancellationToken ct)
    {
        await HandleBatchAsync(scope, ct);
    }

    protected virtual async Task HandleBatchAsync(IServiceScope scope, CancellationToken ct)
    {
        using var childScope = scope.CreateChildScope(true);
        var ids = await GetIdsAsync(childScope, ct);

        foreach (var id in ids)
        {
            using var childScopeId = scope.CreateChildScope(true);
            await HandleIdAsync(childScopeId, id, ct);
        }

        if (ids.Count == _hostedServicesConfiguration.BatchSize)
        {
            await HandleBatchAsync(scope, ct);
        }
    }

    protected abstract Task<List<Guid>> GetIdsAsync(IServiceScope scope, CancellationToken ct);
    protected abstract Task HandleIdAsync(IServiceScope scope, Guid id, CancellationToken ct);
}
