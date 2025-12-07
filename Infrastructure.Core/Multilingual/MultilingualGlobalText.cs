using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.SupportIndex;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual;

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

        builder.HasData(new List<MultilingualGlobalText>()
            {
                new ()
                {
                    Id = new Guid("67BE8513-7ACD-41ED-9CF6-BC554DFA90DC"),
                    Key = "ValidationError.Message",
                    Text = "Validierungsfehler sind aufgetreten.",
                    CultureId = MultilingualCultureIds.De,
                },
                new ()
                {
                    Id = new Guid("72A7FD7A-36B2-4F3C-B699-595392DE0465"),
                    Key = "ValidationError.Message",
                    Text = "Validation error occurred.",
                    CultureId = MultilingualCultureIds.En,
                },
        });
    }
}