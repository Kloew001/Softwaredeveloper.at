using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries;

public class ChronologyEntryDto : Dto
{
    public int? ChronologyType { get; set; }
    public string Description { get; set; }
    public string Text { get; set; }
    public DateTime DateCreated { get; set; }
    public string CreatedByDisplayName { get; set; }
    public string CreatedByShortName { get; set; }
}

public class ChronologyEntryDtoFactory : IDtoFactory<ChronologyEntryDto, ChronologyEntry>
{
    public ChronologyEntryDto ConvertToDto(ChronologyEntry entity, ChronologyEntryDto dto, IServiceProvider serviceProvider)
    {
        var multilingualService =
            serviceProvider.GetService<MultilingualService>();

        dto.Id = entity.Id;

        dto.ChronologyType = entity.ChronologyType;

        dto.Description = multilingualService.GetText(entity, _ => _.Description);
        dto.Text = multilingualService.GetText(entity, _ => _.Text);

        dto.DateCreated = entity.DateCreated;
        dto.CreatedByDisplayName = entity.CreatedBy.UserName;
        dto.CreatedByShortName = GetShortName(entity.CreatedBy);

        return dto;
    }

    private string GetShortName(ApplicationUser user)
    {
        return (user.FirstName.IsNullOrWhiteSpace() ? "" : user.FirstName.Substring(0, 1).ToUpper()) +
               (user.LastName.IsNullOrWhiteSpace() ? "" : user.LastName.Substring(0, 1).ToUpper());
    }

    public ChronologyEntry ConvertToEntity(ChronologyEntryDto dto, ChronologyEntry entity, IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }
}