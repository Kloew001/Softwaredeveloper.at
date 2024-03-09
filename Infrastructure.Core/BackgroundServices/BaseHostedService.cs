using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices
{
    public enum BackgroundserviceInfoStateType
    {
        None = 0,
        Executing = 1,
        Queued = 3,
        Error = 4,
    }

    [Table(nameof(BackgroundserviceInfo))]
    public class BackgroundserviceInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public Guid Id { get; set; }
        public string Name { get; set; }

        public BackgroundserviceInfoStateType State { get; set; } = BackgroundserviceInfoStateType.None;

        public DateTime ExecutedAt { get; set; }
        public DateTime? LastFinishedAt { get; set; }

        public DateTime? NextExecuteAt { get; set; }

        public string Message { get; set; }
        public DateTime? LastErrorAt { get; set; }
        public string LastErrorMessage { get; set; }
        public string LastErrorStack { get; set; }
    }

    public class BackgroundserviceInfoConfiguration : IEntityTypeConfiguration<BackgroundserviceInfo>
    {
        public void Configure(EntityTypeBuilder<BackgroundserviceInfo> builder)
        {
            builder.HasIndex(_ => new
            {
                _.Name,
            }).IsUnique();
        }
    }

    public abstract class BaseHostedService : IHostedService, IDisposable
    {
        protected readonly IServiceScopeFactory _serviceScopeFactory;
        protected readonly ILogger<BaseHostedService> _logger;
        protected readonly IApplicationSettings _applicationSettings;
        protected readonly HostedServicesConfiguration _hostedServicesConfiguration;
        protected readonly IHostApplicationLifetime _appLifetime;
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public virtual string Name { get => GetType().Name; }

        public bool BackgroundServiceInfoEnabled { get; set; } = true;

        protected BaseHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IHostApplicationLifetime appLifetime,
            ILogger<BaseHostedService> logger,
            IApplicationSettings applicationSettings)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _appLifetime = appLifetime;
            _logger = logger;

            _applicationSettings = applicationSettings;
            _applicationSettings.HostedServices.TryGetValue(GetType().Name, out _hostedServicesConfiguration);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"IHostedService.StartAsync for {Name}");

            if (CanStart() == false)
            {
                _logger.LogWarning($"IHostedService {Name} do not have configuration");
                return Task.CompletedTask;
            }

            _appLifetime.ApplicationStarted.Register(async () =>
                await ExecuteAsync(cancellationToken));

            return Task.CompletedTask;
        }

        protected virtual bool CanStart()
        {
            if (_hostedServicesConfiguration == null ||
                _hostedServicesConfiguration?.Enabled != true)
            {
                _logger.LogWarning($"IHostedService {Name} do not have configuration");
                return false;
            }

            if (_hostedServicesConfiguration.EnabledFromTime.HasValue || _hostedServicesConfiguration.EnabledToTime.HasValue)
            {
                if (!_hostedServicesConfiguration.EnabledFromTime.HasValue || !_hostedServicesConfiguration.EnabledToTime.HasValue)
                    _logger.LogError($"IHostedService {Name} EnabledFromTime and EnabledToTime must have value");
            }

            return true;
        }

        protected virtual async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"IHostedService execute for {Name}");

                using (var scope = _serviceScopeFactory.CreateScope())
                using (var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>())
                {
                    if (distributedLock.TryAcquireLock($"{nameof(BackgroundserviceInfo)}_{Name}", 3) == false)
                        throw new InvalidOperationException();

                    try
                    {
                        if (await CanStartBackgroundServiceInfo() == false)
                            return;

                        await StartBackgroundServiceInfo();

                        using (var scopeInner = _serviceScopeFactory.CreateScope())
                        {
                            await ExecuteInternalAsync(scopeInner, cancellationToken);
                        }

                        await FinishedBackgroundServiceInfo();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error while executing background hosted service: '{Name}'");

                        await ErrorBackgroundServiceInfo(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatal Error: '{Name}'");
            }
        }

        protected abstract Task ExecuteInternalAsync(IServiceScope scope, CancellationToken cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"IHostedService.StopAsync for {Name}");

            _cancellationToken.Cancel();

            Dispose();

            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
        }

        protected virtual async Task<bool> CanStartBackgroundServiceInfo()
        {
            if(BackgroundServiceInfoEnabled == false)
                return true;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<IDbContext>();

                var backgroundServiceInfo = await GetBackgroundServiceInfo(context);

                if (_hostedServicesConfiguration.Enabled == false)
                    return false;

                if(backgroundServiceInfo?.State == BackgroundserviceInfoStateType.Executing)
                    return false;

                var now = DateTime.Now;
                if (backgroundServiceInfo == null || backgroundServiceInfo.NextExecuteAt == null)
                {
                    var nextExecuteAt = CalcNextExecuteAt(now, backgroundServiceInfo);
                    return now >= nextExecuteAt;
                }

                if (backgroundServiceInfo?.NextExecuteAt <= now)
                    return true;

                return false;
            }
        }

        protected virtual async Task StartBackgroundServiceInfo()
        {
            if(BackgroundServiceInfoEnabled == false)
                return;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<IDbContext>();

                var backgroundServiceInfo = await GetBackgroundServiceInfo(context);

                if (backgroundServiceInfo == null)
                {
                    backgroundServiceInfo = await context.CreateEntity<BackgroundserviceInfo>();
                    backgroundServiceInfo.Name = Name;
                }

                backgroundServiceInfo.State = BackgroundserviceInfoStateType.Executing;
                backgroundServiceInfo.ExecutedAt = DateTime.Now;
                backgroundServiceInfo.NextExecuteAt = null;
                backgroundServiceInfo.Message = $"started";

                await context.SaveChangesAsync();
            }
        }

        protected virtual async Task FinishedBackgroundServiceInfo()
        {
            if (BackgroundServiceInfoEnabled == false)
                return;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var now = DateTime.Now;

                var context = scope.ServiceProvider.GetService<IDbContext>();

                var backgroundServiceInfo = await GetBackgroundServiceInfo(context);

                backgroundServiceInfo.State = BackgroundserviceInfoStateType.Queued;
                backgroundServiceInfo.LastFinishedAt = now;

                backgroundServiceInfo.NextExecuteAt =
                    CalcNextExecuteAt(now, backgroundServiceInfo);

                backgroundServiceInfo.Message = $"finished";

                await context.SaveChangesAsync();
            }
        }

        protected virtual async Task ErrorBackgroundServiceInfo(Exception ex)
        {
            if (BackgroundServiceInfoEnabled == false)
                return;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<IDbContext>();
                var backgroundServiceInfo = await GetBackgroundServiceInfo(context);

                backgroundServiceInfo.State = BackgroundserviceInfoStateType.Error;
                backgroundServiceInfo.Message = $"error";
                backgroundServiceInfo.LastFinishedAt = null;
                backgroundServiceInfo.LastErrorAt = DateTime.Now;
                backgroundServiceInfo.LastErrorMessage = ex.Message;
                backgroundServiceInfo.LastErrorStack = ex.StackTrace;

                if (_hostedServicesConfiguration.Interval.HasValue)
                {
                    backgroundServiceInfo.NextExecuteAt =
                        CalcNextExecuteAt(DateTime.Now, backgroundServiceInfo);
                }

                await context.SaveChangesAsync();
            }
        }

        private IEnumerable<DateRange> GetEnabledDateRange(DateTime dateTime)
        {
            if (_hostedServicesConfiguration.EnabledFromTime.HasValue &&
                _hostedServicesConfiguration.EnabledToTime.HasValue)
            {
                var enabledFromTime = _hostedServicesConfiguration.EnabledFromTime.Value;
                var enabledToTime = _hostedServicesConfiguration.EnabledToTime.Value;
                var isDayOverlapped = enabledToTime < enabledFromTime;

                if (!isDayOverlapped)
                {
                    var enabledDateRange = new DateRange(
                       dateTime.Date.Add(enabledFromTime),
                       dateTime.Date.Add(enabledToTime));

                    yield return enabledDateRange;
                }
                else
                {
                    var enabledDateRange = new DateRange(
                       dateTime.Date,
                       dateTime.Date.Add(enabledToTime));

                    yield return enabledDateRange;

                    enabledDateRange = new DateRange(
                       dateTime.Date.Add(enabledFromTime),
                       dateTime.Date.AddDays(1).Subtract(TimeSpan.FromSeconds(1)));

                    yield return enabledDateRange;
                }
            }
        }

        private DateTime CalcNextExecuteAt(DateTime dateTime, BackgroundserviceInfo backgroundserviceInfo)
        {
            var enabledDateRanges = GetEnabledDateRange(dateTime).ToList();

            var nextExecuteAt = dateTime.Add(_hostedServicesConfiguration.Interval.Value);

            var isFirstRun = backgroundserviceInfo?.LastFinishedAt == null;

            //backgroundserviceInfo?.NextExecuteAt == null
            if (!enabledDateRanges.Any())
            {
                if (isFirstRun)
                {
                    return dateTime;
                }
                else
                {
                    return nextExecuteAt;
                }
            }
            else
            {
                if (isFirstRun)
                {
                    if (enabledDateRanges.Any(_ => _.Includes(dateTime)))
                    {
                        return dateTime;
                    }
                }

                if (enabledDateRanges.Any(_ => _.Includes(nextExecuteAt)))
                {
                    return nextExecuteAt;
                }
                else
                {
                    var start = enabledDateRanges
                        .Select(_ => _.Start)
                        .OrderByDescending(_ => _)
                        .First();

                    if (start > dateTime)
                        return start;

                    var startNextDay = GetEnabledDateRange(dateTime.Date.AddDays(1).Date)
                        .Select(_ => _.Start)
                        .OrderByDescending(_ => _)
                        .First();

                    return startNextDay;
                }
            }

            //throw new InvalidOperationException();
        }

        private async Task<BackgroundserviceInfo> GetBackgroundServiceInfo(IDbContext context)
        {
            return await
                context.Set<BackgroundserviceInfo>()
                .FirstOrDefaultAsync(_ => _.Name == Name);
        }
    }
}
