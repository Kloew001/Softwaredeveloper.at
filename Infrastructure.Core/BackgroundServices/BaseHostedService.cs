using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices
{
    [Table(nameof(BackgroundserviceInfo))]
    public class BackgroundserviceInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public Guid Id { get; set; }
        public string Name { get; set; }

        public DateTime ExecutedAt { get; set; }
        public DateTime? LastFinishedAt { get; set; }

        public DateTime? NextExecuteAt { get; set; }

        public string Message { get; set; }

        public DateTime? LastErrorAt { get; set; }
        public string LastErrorMessage { get; set; }
        public string LastErrorStack { get; set; }
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

                try
                {
                    if (await CanStartBackgroundServiceInfo() == false)
                        return;

                    await StartBackgroundServiceInfo();

                    using (var scope = _serviceScopeFactory.CreateScope())
                    using (var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>())
                    {
                        if (distributedLock.TryAcquireLock(Name, 3) == false)
                            throw new InvalidOperationException();

                        await ExecuteInternalAsync(scope, cancellationToken);
                    }

                    await FinsihedBackgroundServiceInfo();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while executing background hosted service: '{Name}'");

                    await ErrorBackgroundServiceInfo(ex);
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
            using (var scope = _serviceScopeFactory.CreateScope())
            using (var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>())
            {
                if (distributedLock.TryAcquireLock($"{nameof(BackgroundserviceInfo)}_{Name}", 3) == false)
                    throw new InvalidOperationException();

                var context = scope.ServiceProvider.GetService<IDbContext>();

                var backgroundServiceInfo = await GetBackgroundServiceInfo(context);

                if (_hostedServicesConfiguration.Enabled == false)
                    return false;

                //if (_hostedServicesConfiguration.EnabledFromTime.HasValue &&
                //    DateTime.Now.TimeOfDay < _hostedServicesConfiguration.EnabledFromTime.Value)
                //{
                //    return false;
                //}

                //if (_hostedServicesConfiguration.EnabledToTime.HasValue &&
                //    DateTime.Now.TimeOfDay > _hostedServicesConfiguration.EnabledToTime.Value)
                //{
                //    return false;
                //}

                if (backgroundServiceInfo == null)
                    return true;

                if (backgroundServiceInfo.NextExecuteAt == null)
                {
                    var nextExecuteAt = CalcNextExecuteAt(DateTime.Now);
                    return DateTime.Now >= nextExecuteAt;
                }
                if (backgroundServiceInfo.NextExecuteAt <= DateTime.Now)
                    return true;

                return false;
            }
        }
        protected virtual async Task StartBackgroundServiceInfo()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            using (var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>())
            {
                if (distributedLock.TryAcquireLock($"{nameof(BackgroundserviceInfo)}_{Name}", 3) == false)
                    throw new InvalidOperationException();

                var context = scope.ServiceProvider.GetService<IDbContext>();

                var backgroundServiceInfo = await GetBackgroundServiceInfo(context);

                if (backgroundServiceInfo == null)
                {
                    backgroundServiceInfo = await context.CreateEntity<BackgroundserviceInfo>();
                    backgroundServiceInfo.Name = Name;
                }

                backgroundServiceInfo.ExecutedAt = DateTime.Now;
                backgroundServiceInfo.NextExecuteAt = null;
                backgroundServiceInfo.Message = $"started";

                await context.SaveChangesAsync();
            }
        }

        protected virtual async Task FinsihedBackgroundServiceInfo()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            using (var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>())
            {
                if (distributedLock.TryAcquireLock($"{nameof(BackgroundserviceInfo)}_{Name}", 3) == false)
                    throw new InvalidOperationException();

                var context = scope.ServiceProvider.GetService<IDbContext>();

                var backgroundServiceInfo = await GetBackgroundServiceInfo(context);

                backgroundServiceInfo.LastFinishedAt = DateTime.Now;

                if (_hostedServicesConfiguration.Interval.HasValue)
                {
                    backgroundServiceInfo.NextExecuteAt =
                        CalcNextExecuteAt();
                }

                backgroundServiceInfo.Message = $"finished";

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

        private DateTime CalcNextExecuteAt(DateTime? dateTime = null)
        {
            if (dateTime == null)
                dateTime = DateTime.Now;

            var enabledDateRanges = GetEnabledDateRange(dateTime.Value).ToList();

            var nextExecuteAt = dateTime.Value.Add(_hostedServicesConfiguration.Interval.Value);

            if (!enabledDateRanges.Any() || 
                enabledDateRanges.Any(_ => _.Includes(nextExecuteAt)))
            {
                return nextExecuteAt;
            }

            if (enabledDateRanges.Any() &&
                enabledDateRanges.All(_ => !_.Includes(nextExecuteAt)))
            {
                var start = enabledDateRanges
                    .Select(_ => _.Start)
                    .OrderByDescending(_ => _)
                    .First();
                return start;
            }

            return nextExecuteAt;
        }

        protected virtual async Task ErrorBackgroundServiceInfo(Exception ex)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            using (var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>())
            {
                if (distributedLock.TryAcquireLock($"{nameof(BackgroundserviceInfo)}_{Name}", 3) == false)
                    throw new InvalidOperationException();

                var context = scope.ServiceProvider.GetService<IDbContext>();
                var backgroundServiceInfo = await GetBackgroundServiceInfo(context);

                backgroundServiceInfo.Message = $"error";
                backgroundServiceInfo.LastFinishedAt = null;
                backgroundServiceInfo.LastErrorAt = DateTime.Now;
                backgroundServiceInfo.LastErrorMessage = ex.Message;
                backgroundServiceInfo.LastErrorStack = ex.StackTrace;

                if (_hostedServicesConfiguration.Interval.HasValue)
                {
                    backgroundServiceInfo.NextExecuteAt =
                        CalcNextExecuteAt();
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task<BackgroundserviceInfo> GetBackgroundServiceInfo(IDbContext context)
        {
            return await
                context.Set<BackgroundserviceInfo>()
                .SingleOrDefaultAsync(_ => _.Name == Name);
        }
    }
}
