using System.Diagnostics;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Monitor;

public interface IMonitorService
{
    Task<bool> IsAlive();
    string GetEnvironmentName();
    string GetApplicationName();
    string GetApplicationVersion();
    Task<DBConnectionInfo> DBConnectionInfo();
}

public class DBConnectionInfo
{
    public string CanConnect { get; set; }

    public string SpeedTestAppToDB { get; set; }
}

public class MonitorService : IMonitorService
{
    protected readonly IDbContext _dbContext;
    protected readonly IHostEnvironment _hostEnvironment;

    public MonitorService(IDbContext dbContext, IHostEnvironment hostEnvironment)
    {
        _dbContext = dbContext;
        _hostEnvironment = hostEnvironment;
    }

    public Task<bool> IsAlive() => Task.FromResult(true);

    public string GetEnvironmentName()
    {
        return _hostEnvironment.EnvironmentName;
    }
    public string GetApplicationName()
    {
        return _hostEnvironment.ApplicationName;
    }

    public string GetApplicationVersion()
    {
        return Assembly.GetEntryAssembly().GetName().Version?.ToString();
    }
    public async Task<DBConnectionInfo> DBConnectionInfo()
    {
        var info = new DBConnectionInfo();

        info.CanConnect = await CanConnect();
        info.SpeedTestAppToDB = await SpeedTestAppToDB();

        return info;
    }

    protected virtual async Task<string> CanConnect()
    {
        try
        {
            return (await _dbContext.Database.CanConnectAsync()).ToString();
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    protected virtual async Task<string> SpeedTestAppToDB()
    {
        var watch = Stopwatch.StartNew();

        var result = await _dbContext.Set<BackgroundserviceInfo>().ToListAsync();

        watch.Stop();

        return $"rows: {result.Count}, time: {watch.ElapsedMilliseconds}ms";
    }
}