using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Multilingual
{
    //[Table(nameof(MultilingualText))]
    //public class MultilingualText : BaseEntity
    //{
    //    public Guid CultureId { get; set; }
    //    public LanguageCulture Culture { get; set; }

    //    public string TextKey { get; set; }
    //    public string Value { get; set; }
    //}
    //public class MultilingualTextConfiguration : IEntityTypeConfiguration<MultilingualText>
    //{
    //    public void Configure(EntityTypeBuilder<MultilingualText> builder)
    //    {
    //        builder.HasIndex(_ => new
    //        {
    //            _.Culture,
    //            _.TextKey,
    //        });
    //    }
    //}

    //public class LanguageCulture : BaseEntity
    //{
    //    public string Name { get; set; }
    //    public string DisplayName { get; set; }

    //    public virtual ICollection<MultilingualText> Multilinguals { get; set; }
    //}

    //public class LanguageCultureConfiguration : IEntityTypeConfiguration<LanguageCulture>
    //{
    //    public void Configure(EntityTypeBuilder<LanguageCulture> builder)
    //    {
    //        builder.HasIndex(_ => new
    //        {
    //            _.Name,
    //        });
    //    }
    //}
}
