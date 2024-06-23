using Infrastructure.Core.Web.Middleware;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

using SoftwaredeveloperDotAt.Infrastructure.Core;

namespace Infrastructure.Core.Web
{
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

            builder.AddBearerAuthentication();

            builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
        }

        public virtual void ConfigureApp(WebApplication app)
        {
            DomainStartup.ConfigureApp(app);

            app.Use(async (context, next) => {
                context.Request.EnableBuffering();

                await next();
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseProblemDetails();

            app.MapControllers();

            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRateLimiter();

            app.UseCors();

            app.UseResponseCompression();
        
            app.UseHsts();

            app.UseMiddleware<SecurityHeadersMiddleware>();

            app.UseHttpsRedirection();
        }
    }
}
