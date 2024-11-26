using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.SoftwaredeveloperDotAtDbContext;

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

    protected override HostedServicesConfiguration GetDefaultConfiguration()
    {
        return new HostedServicesConfiguration
        {
            Interval = TimeSpan.FromMinutes(15),
            BatchSize = 100
        };
    }

    protected override async Task<List<Guid>> GetIdsAsync(CancellationToken ct)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<IDbContext>();

        var ids = await context.Set<BinaryContent>()
                .Where(_ => _.ExtractionHandledAt == null ||
                            _.ExtractionHandledAt < _.DateModified)
                .OrderBy(_ => _.DateCreated)
                .Take(_hostedServicesConfiguration.BatchSize)
                .Select(_ => _.Id)
                .ToListAsync(ct);

        return ids;
    }
    protected override async Task HandleIdAsync(Guid id, CancellationToken ct)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();

            scope.ServiceProvider.GetService<ICurrentUserService>()
                .SetCurrentUserId(ApplicationUserIds.ServiceAdminId);

            var context = scope.ServiceProvider.GetService<IDbContext>();

            var binaryContent = await context.Set<BinaryContent>().SingleOrDefaultAsync(_ => _.Id == id);

            var textExtractor = scope.ServiceProvider.GetKeyedService<ITextExtractor>(binaryContent.MimeType);

            var transactionService = scope.ServiceProvider.GetService<DbContextTransaction>();
            transactionService.EnsureTime();

            var transactionDateTime = transactionService.TransactionTime.Value;

            binaryContent.ExtractionHandledAt = transactionDateTime;

            if (textExtractor != null)
                binaryContent.ExtractedText = textExtractor.ExtractText(binaryContent.Content);
            
            await context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            using var scope = _serviceScopeFactory.CreateScope();

            scope.ServiceProvider.GetService<ICurrentUserService>().SetCurrentUserId(ApplicationUserIds.ServiceAdminId);

            var context = scope.ServiceProvider.GetService<IDbContext>();

            var binaryContent = await context.Set<BinaryContent>().SingleOrDefaultAsync(_ => _.Id == id);
            binaryContent.ExtractionHandledAt = DateTime.Now;

            await context.SaveChangesAsync(ct);
        }
    }
}