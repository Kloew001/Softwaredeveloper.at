using Microsoft.Extensions.Hosting;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class HostEnvironmentEnvExtensions
{
    public static bool IsTest(this IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.IsEnvironment(Environments.Test);
    }
}

public static class Environments
{
    public static readonly string Test = "Test";
}