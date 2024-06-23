using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using TomLonghurst.ReadableTimeSpan;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface IDomainStartupCore
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment);
        void ConfigureApp(IHost host);
    }

    public class DomainStartupCore<TApplicationSettings> : IDomainStartupCore
        where TApplicationSettings : class, IApplicationSettings, new()
    {
        protected IConfiguration Configuration { get; set; }
        protected IHostEnvironment HostEnvironment { get; set; }
        protected IServiceCollection Services { get; set; }

        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Services = services;
            Configuration = configuration;
            HostEnvironment = hostEnvironment;

            ApplicationUserIds.ServiceAdminId = Guid.Parse(Configuration.GetSection("ServiceUser").GetValue<string>("Id"));

            if (ApplicationUserIds.ServiceAdminId == Guid.Empty)
                throw new InvalidOperationException("ApplicationUserIds.ServiceAdminId must be set in the DomainStartup.ConfigureServices method.");

            ValidatorOptions.Global.LanguageManager = new ValidationLanguageManager();
            ValidatorOptions.Global.LanguageManager.Enabled = false;

            ReadableTimeSpan.EnableConfigurationBinding();

            ConfigureApplicationSettings();

            Services.AddHttpClient();
            Services.AddMemoryCache();

            Services.RegisterSelfRegisterDependencies();
            Services.RegisterAllHostedService();

            Services.AddSingleton<ICacheService, CacheService>();

            Services.AddSingleton<IEmailMessageGlobalBookmark, EmailMessageGlobalBookmark>();
            Services.AddScoped<IEmailMessageBookmarkReplacer, EmailMessageBookmarkReplacer>();

            Services.AddScoped<IEMailSendHandler, EMailSendHandler>();
            Services.AddScoped<IEMailSender, NoEmailSender>();

            Services.AddScoped<IMonitorService, MonitorService>();

            Services.AddScoped<IApplicationUserService>((sp) =>
            {
                return sp.GetRequiredService<ApplicationUserService>();
            });

            if (HostEnvironment == null || HostEnvironment.IsDevelopment())
                Services.AddScoped<ICurrentUserService, CurrentUserService>();

        }

        protected virtual void ConfigureApplicationSettings()
        {
            Services.Configure<TApplicationSettings>(Configuration);

            Services.AddSingleton<IApplicationSettings>((sp) =>
            {
                return sp.GetRequiredService<IOptionsMonitor<TApplicationSettings>>().CurrentValue;
            });
            Services.AddSingleton((sp) =>
            {
                return sp.GetService<IApplicationSettings>().As<TApplicationSettings>();
            });

            typeof(TApplicationSettings)
                .GetProperties()
                .Where(p => p.PropertyType.GetAttribute<ApplicationConfigurationAttribute>() != null)
                .ToList()
            .ForEach(property =>
            {
                Services.AddSingleton(property.PropertyType, (sp) =>
                {
                    var applicationSettings = sp.GetRequiredService<IApplicationSettings>();

                    return property.GetValue(applicationSettings);
                });
            });
        }

        public virtual void ConfigureApp(IHost host)
        {
            host.UseDtoFactory();

            UpdateDatabase(host);

            AppStartInit(host);
        }

        protected virtual void AppStartInit(IHost host)
        {
            var startupInits =
            host.Services.GetServices<IAppStatupInit>()
                .ToList();

            var tasks = new List<Task>();

            foreach (var item in startupInits)
            {
                tasks.Add(item.Init());
            }

            Task.WhenAll(tasks).GetAwaiter().GetResult();
        }

        protected virtual void UpdateDatabase(IHost host)
        {
            //try
            //{
            using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
                var handler = scope.ServiceProvider.GetRequiredService<IDbContextHandler>();


                handler.UpdateDatabaseAsync(dbContext.As<DbContext>())
                    .GetAwaiter()
                    .GetResult();
            }
            //}
            //catch(Exception ex)
            //{
            //    //ignore
            //}
        }
    }
}
