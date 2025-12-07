using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChangeTracked;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries;

[Table(nameof(ChronologyEntry))]
public class ChronologyEntry : ChangeTrackedEntity, IMultiLingualEntity<ChronologyEntryTranslation>, IReferencedToEntity
{
    public Guid? ReferenceId { get; set; }

    [NotMapped]
    public virtual Entity Reference { get; set; }

    public string ReferenceType { get; set; }

    public int? ChronologyType { get; set; }

    public virtual ICollection<ChronologyEntryTranslation> Translations { get; set; }
}

public class ChronologyEntryConfiguration : IEntityTypeConfiguration<ChronologyEntry>
{
    public void Configure(EntityTypeBuilder<ChronologyEntry> builder)
    {
        builder.Navigation(_ => _.CreatedBy).AutoInclude();
    }
}

[Table(nameof(ChronologyEntryTranslation))]
public class ChronologyEntryTranslation : EntityTranslation<ChronologyEntry>
{
    public string Description { get; set; }
    public string Text { get; set; }
}