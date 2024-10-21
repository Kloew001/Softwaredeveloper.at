
using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

namespace SampleApp.Application.Sections.PersonSection;

public class PersonDtoFactory : IDtoFactory<PersonDto, Person>
{
    public PersonDto ConvertToDto(Person entity, PersonDto dto, IServiceProvider serviceProvider)
    {
        dto.Id = entity.Id;
        dto.FirstName = entity.FirstName;
        dto.LastName = entity.LastName;

        return dto;
    }

    public Person ConvertToEntity(PersonDto dto, Person entity, IServiceProvider serviceProvider)
    {
        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;

        return entity;
    }
}
