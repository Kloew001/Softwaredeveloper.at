using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices
{
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
            await HandleBatchAsync(ct);
        }

        protected virtual async Task HandleBatchAsync(CancellationToken ct)
        {
            var ids = await GetIdsAsync(ct);

            foreach (var id in ids)
            {
                await HandleIdAsync(id, ct);
            }

            if (ids.Count == _hostedServicesConfiguration.BatchSize)
            {
                await HandleBatchAsync(ct);
            }
        }

        protected abstract Task<List<Guid>> GetIdsAsync(CancellationToken ct);
        protected abstract Task HandleIdAsync(Guid id, CancellationToken ct);
    }
}
