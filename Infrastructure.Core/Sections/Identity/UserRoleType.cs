using ExtendableEnums;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;

public class UserRoleType : ExtendableEnum<UserRoleType>
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public UserRoleType(int value, string displayName, Guid id, string name)
        : base(value, displayName)
    {
        Id = id;
        Name = name;
    }
}