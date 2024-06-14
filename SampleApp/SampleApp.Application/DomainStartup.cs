﻿
using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SampleApp.Application
{
    public class DomainStartup : DomainStartupCore<ApplicationSettings>
    {
        public override void ConfigureServices(IHostApplicationBuilder builder)
        {
            var services = builder.Services;

            base.ConfigureServices(builder);

            builder.UsePostgreSQLDbContextHandler();

            services.RegisterDBContext<SampleAppContext>();
        }
    }
}
