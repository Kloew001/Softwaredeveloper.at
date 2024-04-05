using Audit.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoftwaredeveloperDotAt.Infrastructure.Core;
using System.Configuration;

namespace Infrastructure.Core.Web
{
    public class WebStartupCore
    {
        public IConfigurationRoot Configuration { get; }
        //public bool ShouldUseWebApiAudit { get; set; } = true;

        public WebStartupCore(IConfigurationRoot configuration)
        {
            Configuration = configuration;
        }

        public virtual void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 5 * 1024 * 1024;
            });

            builder.AddDefaultServices();

            builder.AddSwaggerGenWithBearer();

            builder.AddBearerAuthentication();

            builder.AddCors();
            builder.AddRateLimiter();

            builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
        }

        public virtual void ConfigureApp(WebApplication app)
        {
            app.Use(async (context, next) => {
                context.Request.EnableBuffering();
                await next();
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHttpsRedirection();

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

            if (app.Environment.IsDevelopment())
                app.UseCors(WebApplicationBuilderExtensions._allowSpecificOrigins);

            app.UseRateLimiter();

            app.UseResponseCompression();

            //if(ShouldUseWebApiAudit)
            //    app.UseAuditMiddleware();
        }
    }
}
