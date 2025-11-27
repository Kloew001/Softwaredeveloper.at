using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Identity;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public bool IsEnabled { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateCreated { get; set; }
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUser", "identity");
    }
}

public class ApplicationRole : IdentityRole<Guid>
{
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("ApplicationRole", "identity");

        var userRoleTypes = UserRoleType.GetAll();

        foreach (var userRoleType in userRoleTypes)
        {
            builder.HasData(new ApplicationRole()
            {
                Id = userRoleType.Id,
                Name = userRoleType.Name,
                NormalizedName = userRoleType.Name.ToUpper()
            });
        }
    }
}

public class ApplicationUserClaim : IdentityUserClaim<Guid>
{

}

public class ApplicationRoleClaimConfiguration : IEntityTypeConfiguration<ApplicationRoleClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
    {
        builder.ToTable("ApplicationRoleClaim", "identity");
    }
}

public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
{

}

public class ApplicationUserToken : IdentityUserToken<Guid>
{

}

public class ApplicationUserLogin : IdentityUserLogin<Guid>
{

}

public class ApplicationUserRole : IdentityUserRole<Guid>
{
    public override Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; }

    public override Guid RoleId { get; set; }
    public virtual ApplicationRole Role { get; set; }
}

public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.ToTable("ApplicationUserRole", "identity");

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        builder.HasOne(ur => ur.User)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
    }
}
