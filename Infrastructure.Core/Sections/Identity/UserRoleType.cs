using ExtendableEnums;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity
{
    public class UserRoleType : ExtendableEnum<UserRoleType>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public UserRoleType(int value, string multilingualDisplayNameKey, Guid id, string name)
            : base(value, multilingualDisplayNameKey)
        {
            Id = id;
            Name = name;
        }
    }
}
