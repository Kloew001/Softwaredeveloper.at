using Infrastructure.Core.Web.Middleware;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SoftwaredeveloperDotAt.Infrastructure.Core;

namespace Infrastructure.Core.Web;

public class WebStartupCore<TDomainStartup>
    where TDomainStartup : IDomainStartupCore, new()
{
    public IConfigurationRoot Configuration { get; }

    public TDomainStartup DomainStartup { get; set; }

    public WebStartupCore(IConfigurationRoot configuration)
    {
        Configuration = configuration;
        DomainStartup = new TDomainStartup();
    }

    public virtual void ConfigureServices(WebApplicationBuilder builder)
    {
        DomainStartup.ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

        builder.AddDefaultServices();

        builder.AddSwaggerGenWithBearer();

        HandleAuthentication(builder);

        builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
    }

    protected virtual void HandleAuthentication(WebApplicationBuilder builder)
    {
        builder.AddJwtBearerAuthentication();
    }

    public virtual void ConfigureApp(WebApplication app)
    {
        DomainStartup.ConfigureApp(app);

        app.UseForwardedHeaders();

        app.UseExceptionHandler();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseResponseCompression();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        HandleRateLimiting(app);

        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();

            await next();
        });

        app.UseCors();

        HandleAuthentication(app);
        HandleAuthorization(app);

        HandleCurrentCulture(app);

        app.UseSecurityHeaders();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            //app.UseSwaggerUI();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
        }

        app.MapControllers();
    }

    protected virtual void HandleAuthorization(WebApplication app)
    {
        app.UseAuthorization();
    }

    protected virtual void HandleAuthentication(WebApplication app)
    {
        app.UseAuthentication();
    }

    protected virtual void HandleRateLimiting(WebApplication app)
    {
        var rateLimitingConfiguration = app.Configuration
            .GetSection("RateLimiting")
            .Get<RateLimitingConfiguration>() ?? new RateLimitingConfiguration();

        if (rateLimitingConfiguration.Enabled)
        {
            app.UseRateLimiter();
        }
    }

    protected virtual void HandleCurrentCulture(WebApplication app)
    {
        app.UseCurrentCulture();
    }
}
