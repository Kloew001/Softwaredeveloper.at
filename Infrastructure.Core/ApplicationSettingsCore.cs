namespace SoftwaredeveloperDotAt.Infrastructure.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ApplicationConfigurationAttribute : Attribute
{
}

public interface IApplicationSettings
{
    Dictionary<string, string> ConnectionStrings { get; }
    RateLimitingConfiguration RateLimiting { get; }
    ForwardedHeadersConfiguration ForwardedHeaders { get; }
    Dictionary<string, HostedServicesConfiguration> HostedServices { get; }
    UrlConfiguration Url { get; }
    SmtpServerConfiguration SmtpServer { get; }
    MultilingualConfiguration Multilingual { get; }
    FeatureToggles FeatureToggles { get; }
    AppLoggingConfiguration AppLogging { get; }
}

public abstract class CoreApplicationSettings : IApplicationSettings
{
    public Dictionary<string, string> ConnectionStrings { get; set; }
    public RateLimitingConfiguration RateLimiting { get; set; } = new();
    public ForwardedHeadersConfiguration ForwardedHeaders { get; set; } = new();
    public Dictionary<string, HostedServicesConfiguration> HostedServices { get; set; }
    public UrlConfiguration Url { get; set; }
    public SmtpServerConfiguration SmtpServer { get; set; }
    public MultilingualConfiguration Multilingual { get; set; } = new MultilingualConfiguration();
    public FeatureToggles FeatureToggles { get; set; } = new FeatureToggles();
    public AppLoggingConfiguration AppLogging { get; set; } = new();
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

    public int? BatchSize { get; set; }

    public HostedServicesExecuteModeType? ExecuteMode { get; set; }
    public TimeSpan? Interval { get; set; }
    public TimeSpan? TriggerExecuteWaitTimeout { get; set; }
}

public enum HostedServicesExecuteModeType
{
    OneTime,
    Interval,
    Trigger
}

[ApplicationConfiguration]
public class RateLimitingConfiguration
{
    public bool Enabled { get; set; } = true;
    public string[] Blacklist { get; set; } = [];
    public string[] Whitelist { get; set; } = [];
}

[ApplicationConfiguration]
public class ForwardedHeadersConfiguration
{
    public string[] KnownProxies { get; set; } = [];
    public string[] KnownNetworks { get; set; } = [];
    public string ForwardedForHeaderName { get; set; }
    public string ForwardedProtoHeaderName { get; set; }
}

[ApplicationConfiguration]
public class FeatureToggles
{
    public bool MonitorDetails { get; set; } = false;
}

[ApplicationConfiguration]
public class AppLoggingConfiguration
{
    public bool EnableExceptionDetails { get; set; } = false;
    public EntityFrameworkLoggingConfiguration EntityFramework { get; set; } = new();
    public FullRequestLoggingConfiguration FullRequestLogging { get; set; } = new();
}

public class EntityFrameworkLoggingConfiguration
{
    public bool EnableDetailedErrors { get; set; } = false;
    public bool EnableSensitiveDataLogging { get; set; } = false;
}

public class FullRequestLoggingConfiguration
{
    public bool Enabled { get; set; } = false;
    public bool IncludeBody { get; set; } = false;
    public int MaxBodyLength { get; set; } = 4096;
    public bool SanitizeSensitiveData { get; set; } = true;
    public string[] SensitiveHeaderNames { get; set; } = [];
    public string[] SensitiveFieldNames { get; set; } = [];
}