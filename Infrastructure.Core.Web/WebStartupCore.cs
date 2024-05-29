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
    public class WebStartupCore
    {

        public IConfigurationRoot Configuration { get; }

        public WebStartupCore(IConfigurationRoot configuration)
        {
            Configuration = configuration;
        }

        public virtual void ConfigureServices(WebApplicationBuilder builder)
        {
            MaxRequestBody(builder);

            builder.AddDefaultServices();

            builder.AddSwaggerGenWithBearer();

            builder.AddBearerAuthentication();

            builder.Services.AddScoped<ICurrentUserService, WebCurrentUserService>();
        }

        private const int DefaultMxRequestBodySizeInMB = 5;

        protected virtual void MaxRequestBody(WebApplicationBuilder builder)
        {
            var maxRequestBodySizeInMB = builder.Configuration.GetSection("MaxRequestBodySizeInMB").Get<int>();

            if (maxRequestBodySizeInMB == 0)
                maxRequestBodySizeInMB = DefaultMxRequestBodySizeInMB;

            builder.Services.Configure(delegate (IISServerOptions options)
            {
                options.MaxRequestBodySize = maxRequestBodySizeInMB * 1024 * 1024;
            });

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = maxRequestBodySizeInMB * 1024 * 1024;
            });
        }

        public virtual void ConfigureApp(WebApplication app)
        {
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
