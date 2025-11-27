namespace SampleApp.Application.Sections.ApplicationUserSection;

public class SampleUserRoleType : ExtendableEnumExtension<UserRoleType>
{
    public static readonly UserRoleType Admin =
        new(0, "Admin", Guid.Parse("7066B0B7-0D2D-4D35-B170-58CD414273EB"), "Admin");

    public static readonly UserRoleType User =
        new(1, "User", Guid.Parse("51B48311-1067-4C4C-BBBB-96F2F29188E2"), "User");
}