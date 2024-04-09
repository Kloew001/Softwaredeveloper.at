using DocumentFormat.OpenXml.Wordprocessing;

using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using SoftwaredeveloperDotAt.Infrastructure.Core.Audit;
using SoftwaredeveloperDotAt.Infrastructure.Core.Dtos;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Monitor;
using SoftwaredeveloperDotAt.Infrastructure.Core.Validation;

using System.Reflection;

using TomLonghurst.ReadableTimeSpan;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public class StartupCore<TApplicationSettings>
        where TApplicationSettings : class, IApplicationSettings, new()
    {
        protected IConfiguration Configuration { get; set; }
        protected IHostEnvironment HostEnvironment { get; set; }
        protected IServiceCollection Services { get; set; }

        public bool ShouldUseDbAudit { get; set; } = false;

        public virtual void ConfigureServices(IHostApplicationBuilder builder)
        {
            Services = builder.Services;
            Configuration = builder.Configuration;
            HostEnvironment = builder.Environment;

            if (ApplicationUserIds.ServiceAdminId == Guid.Empty)
                throw new InvalidOperationException("ApplicationUserIds.ServiceAdminId must be set in the DomainStartup.ConfigureServices method.");

            ValidatorOptions.Global.LanguageManager = new ValidationLanguageManager();

            ReadableTimeSpan.EnableConfigurationBinding();

            Services.Configure<TApplicationSettings>(Configuration);

            Services.AddSingleton<IApplicationSettings>((sp) =>
            {
                var value = sp
                .GetRequiredService<IOptionsMonitor<TApplicationSettings>>()
                .CurrentValue;

                return value;
            });

            Services.AddHttpClient();

            Services.RegisterSelfRegisterDependencies();
            Services.RegisterAllHostedService();

            Services.AddScoped<IEMailSender, NoEmailSender>();
            Services.AddScoped<IMonitorService, MonitorService>();

            if (HostEnvironment == null || HostEnvironment.IsDevelopment())
                Services.AddScoped<ICurrentUserService, CurrentUserService>();

        }

        public virtual void ConfigureApp(IHost host)
        {
            host.UseDtoFactory();

            host.UseAudit();

            UpdateDatabase(host);

            AppStartInit(host);
        }

        protected virtual void AppStartInit(IHost host)
        {
            var startupInits =
            host.Services.GetServices<IAppStatupInit>()
                .ToList();

            foreach (var item in startupInits)
            {
                item.Init()
                    .GetAwaiter()
                    .GetResult();
            }
        }

        protected virtual void UpdateDatabase(IHost host)
        {
            try
            {
                using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
                    var handler = scope.ServiceProvider.GetRequiredService<IDbContextHandler>();


                    handler.UpdateDatabaseAsync(dbContext.As<DbContext>())
                        .GetAwaiter()
                        .GetResult();
                }
            }
            catch
            {
                //ignore
            }
        }
    }
}
