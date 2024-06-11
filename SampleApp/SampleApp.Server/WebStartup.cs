using Infrastructure.Core.Web;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SampleApp.Application;
using SampleApp.Application.Sections.ApplicationUserSection;

using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Authorization;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;

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
        builder.AddDefaultServices();

        // do not remove, to load Dll
        var dbContextHandler =
            new SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.PostgreSQLDbContextHandler();

        DomainStartup.ConfigureServices(builder);

        builder.Services.AddDbContext<SampleAppIdentityDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DbContextConnection");
            options.UseNpgsql(connectionString);
        });

        builder.AddIdentity<ApplicationUser, ApplicationRole, SampleAppIdentityDbContext>();

        builder.Services.AddScoped<ICurrentUserService, AlwaysServiceUserCurrentUserService>();
        //builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();

        builder.AddSwaggerGenWithBearer();

        builder.AddBearerAuthentication(authorizationBuilder =>
        {
            authorizationBuilder
                .AddPolicy("api", policy =>
                {
                    policy.Requirements.Add(new AllowAnonymousAuthorizationRequirement());

                    //policy.RequireAuthenticatedUser();
                    //policy.AddAuthenticationSchemes(IdentityConstants.BearerScheme);
                });
        });
    }

    public override void ConfigureApp(WebApplication app)
    {
        DomainStartup.ConfigureApp(app);

        base.ConfigureApp(app);
    }
}
