using Infrastructure.Core.Web;

using SampleApp.Application;

using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization;

public class WebStartup : WebStartupCore
{
    public DomainStartup DomainStartup { get; set; }
    public WebStartup(IConfigurationRoot configuration)
        : base(configuration)
    {
        DomainStartup = new DomainStartup();
    }

    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        MaxRequestBody(builder);

        builder.AddDefaultServices();

        builder.AddSwaggerGenWithBearer();

        //builder.AddBearerAuthentication();

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("api", policy =>
                policy.Requirements.Add(new AllowAnonymousAuthorizationRequirement())
            );

        var s = new SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.PostgreSQLDbContextHandler(); // do not remove, to load Dll

        DomainStartup.ConfigureServices(builder);

        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        //builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
    }

    public override void ConfigureApp(WebApplication app)
    {
        DomainStartup.ConfigureApp(app);

        base.ConfigureApp(app);
    }
}
