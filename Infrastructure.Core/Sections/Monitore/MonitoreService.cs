using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Monitore
{
    public interface IMonitoreService
    {
        Task<bool> IsAlive();
        Task<DBConnectionInfo> DBConnectionInfo();
    }

    public class DBConnectionInfo
    {
        public string CanConnect { get; set; }

        public string SpeedTestAppToDB { get; set; }
    }

    public class MonitoreService : IMonitoreService
    {
        protected readonly IDbContext _dbContext;

        public MonitoreService(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> IsAlive() => Task.FromResult(true);

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

            var result = await _dbContext.Set<ApplicationUser>().ToListAsync();

            watch.Stop();

            return $"rows: {result.Count}, time: {watch.ElapsedMilliseconds}ms";
        }
    }
}
