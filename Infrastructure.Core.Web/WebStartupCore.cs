using Infrastructure.Core.Web.Middleware;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

using SoftwaredeveloperDotAt.Infrastructure.Core;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;

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

            builder.AddJwtBearerAuthentication();


            builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
        }

        public virtual void ConfigureApp(WebApplication app)
        {
            DomainStartup.ConfigureApp(app);

            app.Use(async (context, next) => {
                context.Request.EnableBuffering();

                await next();
            });

            app.UseHttpsRedirection();

            app.UseExceptionHandler();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapControllers();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                //app.UseSwaggerUI();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
            }

            app.UseRateLimiter();

            app.UseCors();

            app.UseResponseCompression();
        
            app.UseHsts();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCurrentCulture();

            app.UseSecurityHeaders();
        }
    }
}
