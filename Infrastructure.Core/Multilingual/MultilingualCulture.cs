using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Activateable;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportDefault;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual
{
    [Table(nameof(MultilingualCulture))]
    public class MultilingualCulture : Entity, ISupportDefault, IActivateable
    {
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
    }

    public class MultilingualCultureIds
    {
        public static Guid De = Guid.Parse("023EB000-7FDF-4EF5-AA76-BC116F59EBEF");
        public static Guid En = Guid.Parse("A0AF7864-0DC1-49A6-A0D8-2F29157B3801");
    }

    public class MultilingualCultureConfiguration : IEntityTypeConfiguration<MultilingualCulture>
    {
        public void Configure(EntityTypeBuilder<MultilingualCulture> builder)
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

            builder.HasData(new MultilingualCulture()
            {
                Id = MultilingualCultureIds.De,
                IsDefault = true,
                IsActive = true,
                Name = "de"
            });

            builder.HasData(new MultilingualCulture()
            {
                Id = MultilingualCultureIds.En,
                IsActive = true,
                Name = "en"
            });
        }
    }
}
