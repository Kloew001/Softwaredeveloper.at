using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportIndex;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
    public enum MultilingualGlobalTextProtectionLevel
    {
        Private = 0,
        Public = 2
    }

    public class MultilingualGlobalText : Entity, ISupportIndex
    {
        public Guid CultureId { get; set; }
        public virtual MultilingualCulture Culture { get; set; }

        public int Index { get; set; }

        public MultilingualGlobalTextProtectionLevel ProtectionLevel
        {
            set
            {
                ViewLevel = value;
                EditLevel = value;
            }
        }

        public MultilingualGlobalTextProtectionLevel ViewLevel { get; set; } = MultilingualGlobalTextProtectionLevel.Public;
        public MultilingualGlobalTextProtectionLevel EditLevel { get; set; } = MultilingualGlobalTextProtectionLevel.Private;

        public string Key { get; set; }

        public string Text { get; set; }
    }

    public class MultilingualTextConfiguration : IEntityTypeConfiguration<MultilingualGlobalText>
    {
        public void Configure(EntityTypeBuilder<MultilingualGlobalText> builder)
        {
            builder.HasIndex(_ => new
            {
                _.CultureId,
                _.Key
            }).IsUnique();
        }
    }

}
