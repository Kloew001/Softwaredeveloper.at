using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.BinaryContentSection;

public class BinaryContentExtractionHostedService : HandleBatchTimeHostedService
{
    public BinaryContentExtractionHostedService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BinaryContentExtractionHostedService> logger,
        IApplicationSettings settings,
        IHostApplicationLifetime appLifetime)
        : base(serviceScopeFactory, appLifetime, logger, settings)
    {
    }

    protected override HostedServicesConfiguration GetConfiguration()
    {
        var config = base.GetConfiguration();

        config.BatchSize ??= 100;
        config.ExecuteMode ??= HostedServicesExecuteModeType.Interval;

        if (config.ExecuteMode == HostedServicesExecuteModeType.Interval)
            config.Interval ??= TimeSpan.FromMinutes(15);

        return config;
    }

    protected override async Task<List<Guid>> GetIdsAsync(IServiceScope scope, CancellationToken ct)
    {
        var context = scope.ServiceProvider.GetService<IDbContext>();

        var ids = await context.Set<BinaryContent>()
                .Where(_ => _.ExtractionHandledAt == null ||
                            _.ExtractionHandledAt < _.DateModified)
                .OrderBy(_ => _.DateCreated)
                .Take(_configuration.BatchSize.Value)
                .Select(_ => _.Id)
                .ToListAsync(ct);

        return ids;
    }
    protected override async Task HandleIdAsync(IServiceScope scope, Guid id, CancellationToken ct)
    {
        try
        {
            scope.ServiceProvider.GetService<ICurrentUserService>()
                .SetCurrentUserId(ApplicationUserIds.ServiceAdminId);

            var context = scope.ServiceProvider.GetService<IDbContext>();

            var binaryContent = await context.Set<BinaryContent>()
                .Include(_ => _.Data)
                .SingleOrDefaultAsync(_ => _.Id == id, ct);

            var textExtractor = scope.ServiceProvider.GetKeyedService<ITextExtractor>(binaryContent.MimeType);

            var transactionService = scope.ServiceProvider.GetService<DbContextTransaction>();
            transactionService.EnsureTime();

            var transactionDateTime = transactionService.TransactionTime.Value;

            binaryContent.ExtractionHandledAt = transactionDateTime;

            if (textExtractor != null)
            {
                var contentBytes = binaryContent.Data?.Bytes;
                if (contentBytes != null)
                {
                    binaryContent.Data.ExtractedText = textExtractor.ExtractText(contentBytes);
                }
            }

            await context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            using var errorScope = _serviceScopeFactory.CreateScope();

            errorScope.ServiceProvider.GetService<ICurrentUserService>().SetCurrentUserId(ApplicationUserIds.ServiceAdminId);

            var context = errorScope.ServiceProvider.GetService<IDbContext>();

            var binaryContent = await context.Set<BinaryContent>().SingleOrDefaultAsync(_ => _.Id == id, ct);
            binaryContent.ExtractionHandledAt = _dateTimeService.Now();

            await context.SaveChangesAsync(ct);
        }
    }
}