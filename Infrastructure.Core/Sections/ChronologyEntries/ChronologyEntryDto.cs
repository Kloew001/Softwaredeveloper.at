using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries
{
    public class ChronologyEntryDto : Dto
    {
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedByDisplayName { get; set; }

    }

    public class ChronologyEntryDtoFactory : IDtoFactory<ChronologyEntryDto, ChronologyEntry>
    {
        public ChronologyEntryDto ConvertToDto(ChronologyEntry entity, ChronologyEntryDto dto, IServiceProvider serviceProvider)
        {
            var multilingualService =
                serviceProvider.GetService<MultilingualService>();

            dto.Id = entity.Id;

            dto.Description = multilingualService
                .GetText(entity, _ => _.Description) ?? entity.Description;

            dto.DateCreated = entity.DateCreated;
            dto.CreatedByDisplayName = entity.CreatedBy.UserName;

            return dto;
        }

        public ChronologyEntry ConvertToEntity(ChronologyEntryDto dto, ChronologyEntry entity, IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
