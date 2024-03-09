using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity
{
    public class ApplicationUserDto : Dto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
