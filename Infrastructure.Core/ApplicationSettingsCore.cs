using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface IApplicationSettings
    { 
        Dictionary<string, string> ConnectionStrings { get; set; }

        Dictionary<string, HostedServicesConfiguration> HostedServices { get; set; }

        EMailServerConfiguration EMailServer { get; set; }
    }

    public abstract class CoreApplicationSettings : IApplicationSettings
    {
        public Dictionary<string, string> ConnectionStrings { get; set; }

        public Dictionary<string, HostedServicesConfiguration> HostedServices { get; set; }

        public EMailServerConfiguration EMailServer { get; set; }
    }

    public class HostedServicesConfiguration
    {
        public bool Enabled { get; set; } = true;
        public TimeSpan? EnabledFromTime { get; set; }
        public TimeSpan? EnabledToTime { get; set; }

        public int BatchSize { get; set; } = 10;

        public TimeSpan? Interval { get; set; }
        public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(10);
    }
}
