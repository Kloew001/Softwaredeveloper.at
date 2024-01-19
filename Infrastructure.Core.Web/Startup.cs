using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoftwaredeveloperDotAt.Infrastructure.Core;
using System.Configuration;

namespace Infrastructure.Core.Web
{
    public class StartupCoreWeb
    {
        public IConfigurationRoot Configuration { get; }
        public StartupCoreWeb(IConfigurationRoot configuration)
        {
            Configuration = configuration;
        }

        public virtual void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.AddDefaultServices();

            builder.AddSwaggerGenWithBearer();

            builder.AddBearerAuthentication();

            builder.AddCors();
            builder.AddRateLimiter();

            //var domainStartup = new DomainStartup();
            //domainStartup.ConfigureServices(builder.Services, builder.Environment, builder.Configuration);

            builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
        }

        public virtual void ConfigureApp(WebApplication app)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (app.Environment.IsDevelopment())
                app.UseCors(WebApplicationBuilderExtensions._allowSpecificOrigins);

            app.UseRateLimiter();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
