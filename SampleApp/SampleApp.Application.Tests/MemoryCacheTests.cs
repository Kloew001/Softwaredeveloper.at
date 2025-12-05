using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SoftwaredeveloperDotAt.Infrastructure.Core.Tests;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility.Cache;

namespace SampleApp.Application.Tests;

public class MemoryCacheTests : BaseTest<DomainStartup>
{
    [Test]
    public void RemoveStartsWith()
    {
        var memoryCache = _serviceScope.ServiceProvider
                .GetRequiredService<IMemoryCache>();

        memoryCache.GetOrCreate("XX_1", (_) =>
        {
            return true;
        });
        memoryCache.GetOrCreate("XX_2", (_) =>
        {
            return true;
        });

        Assert.That(memoryCache.Get<bool?>("XX_1") == true);
        Assert.That(memoryCache.Get<bool?>("XX_2") == true);

        memoryCache.RemoveStartsWith("XX_");

        Assert.That(memoryCache.Get<bool?>("XX_1") == null);
        Assert.That(memoryCache.Get<bool?>("XX_2") == null);
    }
}
