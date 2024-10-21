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
        DomainStartup.ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

        builder.AddDefaultServices();

        builder.Services.RegisterDBContext<SampleAppIdentityDbContext>();
        builder.AddJwtBearerAuthentication();

        //builder.AddJwtBearerAuthentication(authorizationBuilder =>
        //{
        //    authorizationBuilder
        //        .AddPolicy("api", policy =>
        //        {
        //            policy.Requirements.Add(new AllowAnonymousAuthorizationRequirement());

        //            //policy.RequireAuthenticatedUser();
        //            //policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        //        });
        //});

        builder.AddSwaggerGenWithBearer();

        builder.AddIdentity<ApplicationUser, ApplicationRole, SampleAppIdentityDbContext>();

        //builder.Services.AddScoped<ICurrentUserService, AlwaysServiceUserCurrentUserService>();
        builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
    }
}
