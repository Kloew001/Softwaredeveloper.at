using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

namespace SampleApp.Application.Sections.PersonSection;

public class PersonDtoFactoryProfile : IDtoFactoryProfile
{
    public void Apply(DtoFactoryConfiguration config)
    {
        config.For<Person, PersonDto>()
            .ForMember(_ => _.FirstName, o => o.IgnoreMember());
    }
}

public class PersonDtoFactory(IMemoryCache memoryCache, DtoFactoryResolver factoryResolver) 
    : DefaultDtoFactory<PersonDto, Person>(memoryCache, factoryResolver)
{
    public override PersonDto ConvertToDto(Person entity, PersonDto dto)
    {
        base.ConvertToDto(entity, dto);

        dto.Id = entity.Id;
        dto.FirstName = entity.FirstName;
        dto.LastName = entity.LastName;

        return dto;
    }

    public override Person ConvertToEntity(PersonDto dto, Person entity)
    {
        base.ConvertToEntity(dto, entity);

        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;

        return entity;
    }
}