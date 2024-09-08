using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChangeTracked;
using MimeKit;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.BinaryContentSection
{
    [Table(nameof(BinaryContent), Schema = "core")]
    public class BinaryContent : ChangeTrackedEntity, IReferencedToEntity
    {
        public string Name { get; set; }

        public Guid? ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        [NotMapped]
        public virtual Entity Reference { get; set; }

        public string MimeType { get; set; }

        public byte[] Content { get; set; }
        public long ContentSize { get; set; }
        public string Description { get; set; }

    }

    public class BinaryContentConfiguration : IEntityTypeConfiguration<BinaryContent>
    {
        public void Configure(EntityTypeBuilder<BinaryContent> builder)
        {
            builder.HasIndex(_ => new
            {
                _.ReferenceId,
                _.ReferenceType
            });
        }
    }

    public static class BinaryContentExtensions
    {
        public static void Fill(this BinaryContent binaryContent, Utility.FileInfo fileInfo)
        {
            binaryContent.Name = fileInfo.FileName;
            binaryContent.MimeType = 
                fileInfo.FileContentType ?? 
                MimeTypes.GetMimeType(fileInfo.FileName); ;

            Fill(binaryContent, fileInfo.Content);
        }

        public static void Fill(this BinaryContent binaryContent, byte[] content)
        {
            binaryContent.Content = content;
            binaryContent.ContentSize = content.Length;
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
