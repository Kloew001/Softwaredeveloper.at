using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

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
        public ChronologyEntryDto ConvertToDto(ChronologyEntry entity, ChronologyEntryDto dto)
        {
            dto.Id = entity.Id;
            dto.Description = entity.Description;
            dto.DateCreated = entity.DateCreated;
            dto.CreatedByDisplayName = entity.CreatedBy.UserName;

            return dto;
        }

        public ChronologyEntry ConvertToEntity(ChronologyEntryDto dto, ChronologyEntry entity)
        {
            throw new NotImplementedException();
        }
    }
}
