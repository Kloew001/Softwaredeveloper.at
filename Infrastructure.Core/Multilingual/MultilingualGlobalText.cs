using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
    public class MultilingualGlobalText : Entity
    {
        public Guid CultureId { get; set; }
        public virtual MultilingualCulture Culture { get; set; }

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
