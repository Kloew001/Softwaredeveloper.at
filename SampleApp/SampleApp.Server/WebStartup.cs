using Infrastructure.Core.Web;

using SampleApp.Application;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;

public class WebStartup : WebStartupCore<DomainStartup>
{
    public WebStartup(IConfigurationRoot configuration)
        : base(configuration)
    {
    }

    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        base.ConfigureServices(builder);

        builder.Services.RegisterDBContext<SampleAppIdentityDbContext>();

        builder.AddIdentity<ApplicationUser, ApplicationRole, SampleAppIdentityDbContext>();

        builder.Services.AddScoped<ICurrentUserService, AlwaysServiceUserCurrentUserService>();
        //builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
    }
}
