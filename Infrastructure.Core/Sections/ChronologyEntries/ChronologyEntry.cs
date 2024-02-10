using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries
{
    [Table(nameof(ChronologyEntry))]
    public class ChronologyEntry : ChangeTrackedEntity
    {
        public string Description { get; set; }
        public Guid? ReferenceId { get; set; }
        public string ReferenceType { get; set; }
    }

    public class ChronologyEntryConfiguration : IEntityTypeConfiguration<ChronologyEntry>
    {
        public void Configure(EntityTypeBuilder<ChronologyEntry> builder)
        {
            builder.HasIndex(_ => new
            {
                _.ReferenceId,
                _.ReferenceType
            });
        }
    }

}
