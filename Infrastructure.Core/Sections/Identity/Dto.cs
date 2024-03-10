﻿using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity
{
    public class ApplicationUserDto : Dto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class ApplicationUserDetailDto : ApplicationUserDto
    {
        public IEnumerable<ApplicationRoleDto> Roles { get; set; }
    }

    public class ApplicationRoleDto : Dto
    {
        public string Name { get; set; }
    }

    public class ApplicationUserDtoFactory : IDtoFactory<ApplicationUserDetailDto, ApplicationUser>
    {
        public ApplicationUserDetailDto ConvertToDto(ApplicationUser entity, ApplicationUserDetailDto dto)
        {
            dto.Id = entity.Id;
            dto.Email = entity.Email;
            dto.FirstName = entity.FirstName;
            dto.LastName = entity.LastName;

            dto.Roles = entity.UserRoles.Select(_ => _.Role)
                .Select(_ => new ApplicationRoleDto
                {
                    Id = _.Id,
                    Name = _.Name
                });

            return dto;
        }

        public ApplicationUser ConvertToEntity(ApplicationUserDetailDto dto, ApplicationUser entity)
        {
            throw new NotImplementedException();
        }
    }
}
