using MimeKit;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.BinaryContentSection;

public interface IReferencedToBinaryContent
{
    public Guid Id { get; set; }
    public Guid? BinaryContentId { get; set; }
    public BinaryContent BinaryContent { get; set; }
}

[ScopedDependency]
public class BinaryContentService
{
    private IDbContext _context;

    public BinaryContentService(IDbContext context)
    {
        _context = context;
    }

    public async Task ApplyContentAsync(IReferencedToBinaryContent referencedEntity, string name, byte[] content, string mimeType = null)
    {
        if (referencedEntity.BinaryContent == null)
        {
            var binaryContent = await _context.CreateEntityAync<BinaryContent>();
            referencedEntity.BinaryContentId = binaryContent.Id;
            referencedEntity.BinaryContent = binaryContent;

            referencedEntity.BinaryContent.ReferenceType = referencedEntity.GetType().UnProxy().Name;
            referencedEntity.BinaryContent.ReferenceId = referencedEntity.Id;
        }

        referencedEntity.BinaryContent.Name = name;
        referencedEntity.BinaryContent.MimeType = mimeType ?? MimeTypes.GetMimeType(name);
        referencedEntity.BinaryContent.Content = content;
        referencedEntity.BinaryContent.ContentSize = content.Length;
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