using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SampleApp.Application
{
    public class DomainStartup : DomainStartupCore<ApplicationSettings>
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            base.ConfigureServices(services, configuration, hostEnvironment);

            services.UsePostgreSQLDbContextHandler(configuration);

            services.RegisterDBContext<SampleAppContext>();

            //Services.AddScoped<IEMailSendHandler, MailToolEMailSendHandler>();
            Services.AddScoped<IEMailSender, SmtpEMailSender>();
        }
    }
}
