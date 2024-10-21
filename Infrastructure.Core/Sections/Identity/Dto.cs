namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;

public class ApplicationUserDto : Dto
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class ApplicationUserDetailDto : ApplicationUserDto
{
    public string PreferedCultureName { get; set; }
    public IEnumerable<ApplicationRoleDto> Roles { get; set; }
}

public class ApplicationRoleDto : Dto
{
    public string Name { get; set; }
}

public class ApplicationUserDtoFactory : IDtoFactory<ApplicationUserDetailDto, ApplicationUser>
{
    public ApplicationUserDetailDto ConvertToDto(ApplicationUser entity, ApplicationUserDetailDto dto, IServiceProvider serviceProvider)
    {
        dto.Id = entity.Id;
        dto.Email = entity.Email;
        dto.FirstName = entity.FirstName;
        dto.LastName = entity.LastName;
        dto.PreferedCultureName = entity.PreferedCulture?.Name;

        dto.Roles = entity.UserRoles.Select(_ => _.Role)
            .Select(_ => new ApplicationRoleDto
            {
                Id = _.Id,
                Name = _.Name
            }).ToList();

        return dto;
    }

    public ApplicationUser ConvertToEntity(ApplicationUserDetailDto dto, ApplicationUser entity, IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }
}
