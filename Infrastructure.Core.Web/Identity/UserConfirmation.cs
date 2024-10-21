using Microsoft.AspNetCore.Identity;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;

public class UserConfirmation : Microsoft.AspNetCore.Identity.DefaultUserConfirmation<ApplicationUser>,  IUserConfirmation<ApplicationUser>
{
    public override async Task<bool> IsConfirmedAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        if(await base.IsConfirmedAsync(manager, user) == false)
            return false;

        return user.IsEnabled;
    }
}
