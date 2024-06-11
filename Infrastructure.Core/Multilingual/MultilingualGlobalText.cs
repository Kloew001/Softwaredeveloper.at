using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportIndex;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
    public enum MultilingualProtectionLevel
    {
        Private = 0,
        Public = 2
    }

    [Table(nameof(MultilingualGlobalText), Schema = "core")]
    public class MultilingualGlobalText : Entity, ISupportIndex
    {
        public Guid CultureId { get; set; }
        public virtual MultilingualCulture Culture { get; set; }

        public int Index { get; set; }

        public MultilingualProtectionLevel ProtectionLevel
        {
            set
            {
                ViewLevel = value;
                EditLevel = value;
            }
        }

        public MultilingualProtectionLevel ViewLevel { get; set; } = MultilingualProtectionLevel.Private;
        public MultilingualProtectionLevel EditLevel { get; set; } = MultilingualProtectionLevel.Private;

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
