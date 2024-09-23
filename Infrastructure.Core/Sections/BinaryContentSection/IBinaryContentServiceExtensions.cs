using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.BinaryContentSection
{
    public static class IBinaryContentServiceExtensions
    {
        public static async Task ApplyContentAsync<TEntity>(
            this EntityService<TEntity> service, 
            IReferencedToBinaryContent referencedEntity, 
            string name, 
            byte[] content,
            string mimeType = null)
            where TEntity : Entity, IReferencedToBinaryContent
        {
            var serviceProvider = service.EntityServiceDependency.ServiceProvider;
            var binaryContentService = serviceProvider.GetService<BinaryContentService>();

            await binaryContentService.ApplyContentAsync(referencedEntity, name, content, mimeType);
        }
    }

    //public class OrphanBinaryContentHostedService : TimerHostedService
    //{
    //    public OrphanBinaryContentHostedService(
    //        IServiceScopeFactory serviceScopeFactory,
    //        ILogger<OrphanBinaryContentHostedService> logger,
    //        IApplicationSettings optionsAccessor,
    //        IHostApplicationLifetime appLifetime)
    //        : base(serviceScopeFactory, appLifetime, logger, optionsAccessor)
    //    {
    //    }

    //    protected override async Task ExecuteInternalAsync(IServiceScope scope, CancellationToken cancellationToken)
    //    {
    //        var context = scope.ServiceProvider.GetService<IDbContext>();

    //        var date = DateTime.Now.AddSeconds(-1 * _hostedServicesConfiguration.InitialDelayInSeconds);

    //        await context.Set<BinaryContent>()
    //            .Where(_ => _.ReferenceType != null &&
    //                        _.ReferenceId != null &&
    //                        _.DateModified > date &&
    //                        context.Database.SqlQuery<bool>
    //                        ($"EXISTS(SELECT _.Id FROM {} WHERE "ReferenceId" = {_.ReferenceId} AND " +
    //                        "   {nameof(BinaryContent.ReferenceType)} = {_.ReferenceType}").First() == 0)
    //                        )
    //            .Take(_hostedServicesConfiguration.BatchSize)
    //            .ExecuteDeleteAsync(cancellationToken);
    //    }
    //}
}
