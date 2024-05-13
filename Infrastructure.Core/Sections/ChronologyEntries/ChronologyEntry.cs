using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChangeTracked;
using SoftwaredeveloperDotAt.Infrastructure.Core.AccessCondition;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries
{
    [Table(nameof(ChronologyEntry))]
    public class ChronologyEntry : ChangeTrackedEntity, IMultiLingualEntity<ChronologyEntryTranslation>, IReferencedToEntity
    {
        public string Description { get; set; }
        public Guid? ReferenceId { get; set; }
        
        [NotMapped]
        public virtual Entity Reference { get; set; }
        
        public string ReferenceType { get; set; }
        
        public virtual ICollection<ChronologyEntryTranslation> Translations { get; set; }
    }

    public class ChronologyEntryConfiguration : IEntityTypeConfiguration<ChronologyEntry>
    {
        public void Configure(EntityTypeBuilder<ChronologyEntry> builder)
        {
        }
    }

    [Table(nameof(ChronologyEntryTranslation))]
    public class ChronologyEntryTranslation : EntityTranslation<ChronologyEntry>
    {
        public string Description { get; set; }
    }

}
