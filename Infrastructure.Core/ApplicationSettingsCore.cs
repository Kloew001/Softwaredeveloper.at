namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface IApplicationSettings
    { 
        Dictionary<string, string> ConnectionStrings { get; set; }

        Dictionary<string, HostedServicesConfiguration> HostedServicesConfiguration { get; set; }

        EMailConfiguration EMailConfiguration { get; set; }
    }

    public abstract class CoreApplicationSettings : IApplicationSettings
    {
        public Dictionary<string, string> ConnectionStrings { get; set; }

        public Dictionary<string, HostedServicesConfiguration> HostedServicesConfiguration { get; set; }

        public EMailConfiguration EMailConfiguration { get; set; }
    }

    public class HostedServicesConfiguration
    {
        public bool Enabled { get; set; } = true;
        public int BatchSize { get; set; } = 10;
        public int IntervalInSeconds { get; set; } = 60;
        public int InitialDelayInSeconds { get; set; } = 10;
    }
}
