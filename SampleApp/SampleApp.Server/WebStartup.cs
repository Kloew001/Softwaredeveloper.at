using SampleApp.Application;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;

public class WebStartup : WebStartupCore<DomainStartup>
{
    public WebStartup(WebApplicationBuilder builder)
        : base(builder)
    {
    }

    protected override void ConfigureServices()
    {
        base.ConfigureServices();

        Services.RegisterDBContext<SampleAppIdentityDbContext>();

        Builder.AddIdentity<ApplicationUser, ApplicationRole, SampleAppIdentityDbContext>();

        Services.AddScoped<ICurrentUserService, AlwaysServiceUserCurrentUserService>();
        //Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
    }
}
