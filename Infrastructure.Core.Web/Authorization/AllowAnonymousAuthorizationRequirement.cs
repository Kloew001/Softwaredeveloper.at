using Microsoft.AspNetCore.Authorization;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization;

public class AllowAnonymousAuthorizationRequirement :
       AuthorizationHandler<AllowAnonymousAuthorizationRequirement>, IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AllowAnonymousAuthorizationRequirement requirement)
    {
        var user = context.User;
        var userIsAnonymous = user?.Identity == null || !user.Identities.Any(i => i.IsAuthenticated);
        if (userIsAnonymous)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
