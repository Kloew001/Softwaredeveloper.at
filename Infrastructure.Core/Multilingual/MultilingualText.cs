using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
    [Table(nameof(LanguageCulture))]
    public class LanguageCulture : Entity, ISupportDefault, IEnableable
    {
        public bool IsDefault { get; set; }
        public bool IsEnabled { get; set; }
        public string Name { get; set; }
    }

    public class LanguageCultureIds
    {
        public static Guid DeAt = Guid.Parse("023EB000-7FDF-4EF5-AA76-BC116F59EBEF");
        public static Guid EnUs = Guid.Parse("A0AF7864-0DC1-49A6-A0D8-2F29157B3801");
    }

    public class LanguageCultureConfiguration : IEntityTypeConfiguration<LanguageCulture>
    {
        public void Configure(EntityTypeBuilder<LanguageCulture> builder)
        {
            builder.HasIndex(_ => new
            {
                _.IsDefault,
            });
            //.HasQueryFilter(p => !p.IsDeleted);
            //.HasFilter("IsEnabled == 1").IsUnique();

            builder.HasIndex(_ => new
            {
                _.Name,
            }).IsUnique();

            builder.HasData(new LanguageCulture()
            {
                Id = LanguageCultureIds.DeAt,
                Name = "de-AT"
            });

            builder.HasData(new LanguageCulture()
            {
                Id = LanguageCultureIds.EnUs,
                Name = "en-US"
            });
        }
    }
}
