
namespace SampleApp.Application;

public class ApplicationSettings : CoreApplicationSettings
{
    public UrlConfiguration Url { get; set; }
}

public class UrlConfiguration
{
    public string BaseUrl { get; set; }
}
