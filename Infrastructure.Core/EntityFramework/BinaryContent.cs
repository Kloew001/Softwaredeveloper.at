using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    [Table(nameof(BinaryContent))]
    public class BinaryContent : BaseEntity
    {
        public string MimeType { get; set; }

        public byte[] Content { get; set; }
    }

    public class BinaryContentConfiguration : IEntityTypeConfiguration<BinaryContent>
    {
        public void Configure(EntityTypeBuilder<BinaryContent> builder)
        {
        }
    }
}
