
namespace SampleApp.Application;

public class ApplicationSettings : CoreApplicationSettings
{
    public SampleConfiguration Sample { get; set; }
}

public class SampleConfiguration
{
    public string SampleMode { get; set; }
}