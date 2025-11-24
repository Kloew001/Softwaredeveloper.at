namespace SoftwaredeveloperDotAt.Infrastructure.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ApplicationConfigurationAttribute : Attribute
{
}

public interface IApplicationSettings
{
    Dictionary<string, string> ConnectionStrings { get; set; }

    Dictionary<string, HostedServicesConfiguration> HostedServices { get; set; }
    UrlConfiguration Url { get; set; }

    SmtpServerConfiguration SmtpServer { get; set; }
    MultilingualConfiguration Multilingual { get; set; }
}

public abstract class CoreApplicationSettings : IApplicationSettings
{
    public Dictionary<string, string> ConnectionStrings { get; set; }
    public RateLimitingConfiguration RateLimiting { get; set; } = new RateLimitingConfiguration();
    public ForwardedHeadersConfiguration ForwardedHeaders { get; set; } = new ForwardedHeadersConfiguration();
    public Dictionary<string, HostedServicesConfiguration> HostedServices { get; set; }
    public UrlConfiguration Url { get; set; }
    public SmtpServerConfiguration SmtpServer { get; set; }
    public MultilingualConfiguration Multilingual { get; set; }
}

[ApplicationConfiguration]
public class UrlConfiguration
{
    public string BaseUrl { get; set; }
}

public class HostedServicesConfiguration
{
    public bool Enabled { get; set; } = true;
    public TimeSpan? EnabledFromTime { get; set; }
    public TimeSpan? EnabledToTime { get; set; }

    public int BatchSize { get; set; } = 10;

    public TimeSpan? Interval { get; set; } = null;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(10);
}

[ApplicationConfiguration]
public class RateLimitingConfiguration
{
    public bool Enabled { get; set; } = true;
}

[ApplicationConfiguration]
public class ForwardedHeadersConfiguration
{
    public string[] KnownProxies { get; set; } = [];
    public string[] KnownNetworks { get; set; } = [];
    public string ForwardedForHeaderName { get; set; }
    public string ForwardedProtoHeaderName { get; set; }
}
