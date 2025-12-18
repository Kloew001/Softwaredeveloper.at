using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

using DocumentFormat.OpenXml.Wordprocessing;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Serilog.Context;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices;

public enum BackgroundserviceInfoStateType
{
    None = 0,
    Executing = 1,
    Queued = 3,
    Error = 4,
}

[Table(nameof(BackgroundserviceInfo), Schema = "core")]
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
    public DateTime? LastHeartbeat { get; set; }
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
    protected readonly HostedServicesConfiguration _configuration;
    protected readonly IHostApplicationLifetime _appLifetime;
    protected readonly IDateTimeService _dateTimeService;
    private readonly CancellationTokenSource _cancellationToken = new();

    public virtual string Name { get => GetType().Name; }

    public bool BackgroundServiceInfoEnabled { get; protected set; } = true;

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
        _dateTimeService = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IDateTimeService>();
        _configuration = GetConfiguration();
        PostPrepareConfiguration();
    }

    protected virtual HostedServicesConfiguration GetConfiguration()
    {
        if (_applicationSettings.HostedServices.TryGetValue(GetType().Name, out var hostedServicesConfiguration))
        {
            return hostedServicesConfiguration;
        }

        return GetDefaultConfiguration();
    }

    protected virtual void PostPrepareConfiguration()
    {
        _configuration.ExecuteMode ??= HostedServicesExecuteModeType.OneTime;
        _configuration.BatchSize ??= 10;
    }

    private HostedServicesConfiguration GetDefaultConfiguration()
    {
        return new HostedServicesConfiguration();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString();
        using (LogContext.PushProperty(SerilogUtility.Area, SerilogUtility.Area_Worker))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("HostedService Start '{name}' with configuration: {configuration}",
                Name,
                _configuration.ToJson(new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    Converters = { new StringEnumConverter() }
                }));

            if (CanStart() == false)
            {
                return Task.CompletedTask;
            }

            _appLifetime.ApplicationStarted.Register(async () =>
                await ExecuteAsync(cancellationToken));

            return Task.CompletedTask;
        }
    }

    protected virtual bool CanStart()
    {
        if (_configuration == null)
        {
            _logger.LogWarning("HostedService {name} do not have configuration.", Name);
            return false;
        }

        if (_configuration?.Enabled != true)
        {
            _logger.LogInformation("HostedService {name} is disabled.", Name);
            return false;
        }

        if (_configuration.EnabledFromTime.HasValue || _configuration.EnabledToTime.HasValue)
        {
            if (!_configuration.EnabledFromTime.HasValue || !_configuration.EnabledToTime.HasValue)
                _logger.LogError("HostedService {name} EnabledFromTime and EnabledToTime must have value", Name);
        }

        return true;
    }

    public virtual async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var stopWatch = Stopwatch.StartNew();

            using var scope = _serviceScopeFactory.CreateScope();
            using var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>();

            if (await distributedLock.TryAcquireLockAsync($"{nameof(BackgroundserviceInfo)}_{Name}", 3, cancellationToken) == false)
                throw new InvalidOperationException();

            try
            {
                if (await CanStartBackgroundServiceInfoAsync(cancellationToken) == false)
                    return;

                _logger.LogInformation("Start BackgroundService: {name}", Name);

                await StartBackgroundServiceInfoAsync(cancellationToken);

                using var heartbeatCancellationTokenSource = new CancellationTokenSource();
                var heartbeatTask = StartHeartbeatAsync(heartbeatCancellationTokenSource.Token);

                using (var scopeInner = _serviceScopeFactory.CreateScope())
                {
                    await ExecuteInternalAsync(scopeInner, cancellationToken);
                }
                heartbeatCancellationTokenSource.Cancel();
                await heartbeatTask;

                await FinishedBackgroundServiceInfoAsync(cancellationToken);

                _logger.LogInformation("Finished BackgroundService: {name} in {ElapsedMilliseconds} ms", Name, stopWatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BackgroundService: {name}", Name);

                await ErrorBackgroundServiceInfoAsync(ex, cancellationToken);
            }
            finally
            {
                await CheckExecutingAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal Error: {name}", Name);
        }
    }

    private async Task CheckExecutingAsync(CancellationToken cancellationToken)
    {
        if (BackgroundServiceInfoEnabled == false)
            return;

        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<IDbContext>();

        var backgroundServiceInfo = await GetBackgroundServiceInfoAsync(context);

        if (IsExecuting(backgroundServiceInfo))
        {
            await ErrorBackgroundServiceInfoAsync(new TimeoutException(), cancellationToken);
        }
    }

    protected abstract Task ExecuteInternalAsync(IServiceScope scope, CancellationToken cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("IHostedService Stop for {name}", Name);

        _cancellationToken.Cancel();

        Dispose();

        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
    }

    protected virtual TimeSpan HearthBeatInterval { get; set; } = TimeSpan.FromSeconds(10);

    private async Task StartHeartbeatAsync(CancellationToken cancellationToken)
    {
        try
        {
            do
            {
                await Task.Delay(HearthBeatInterval, cancellationToken);

                await UpdateHeartbeatBackgroundServiceInfoAsync(cancellationToken);
            }
            while (!cancellationToken.IsCancellationRequested);
        }
        catch (TaskCanceledException)
        {
            //ignore
        }
        catch (Exception ex)
        {
            throw new Exception($"Heartbeat error: {ex.Message}", ex);
        }
    }

    protected virtual async Task<bool> CanStartBackgroundServiceInfoAsync(CancellationToken cancellationToken)
    {
        if (BackgroundServiceInfoEnabled == false)
            return true;

        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<IDbContext>();

        var backgroundServiceInfo = await GetBackgroundServiceInfoAsync(context);

        if (_configuration.Enabled == false)
            return false;

        if (IsTimeouted(backgroundServiceInfo))
            return true;

        if (IsExecuting(backgroundServiceInfo))
            return false;

        var now = _dateTimeService.Now();
        if (backgroundServiceInfo == null || backgroundServiceInfo.NextExecuteAt == null)
        {
            var nextExecuteAt = CalcNextExecuteAt(now, backgroundServiceInfo);
            return now >= nextExecuteAt;
        }

        if (backgroundServiceInfo?.NextExecuteAt <= now)
            return true;

        return false;
    }

    protected bool IsExecuting(BackgroundserviceInfo backgroundServiceInfo)
    {
        return backgroundServiceInfo?.State == BackgroundserviceInfoStateType.Executing;
    }

    protected virtual bool IsTimeouted(BackgroundserviceInfo backgroundServiceInfo)
    {
        if (IsExecuting(backgroundServiceInfo))
        {
            var now = _dateTimeService.Now();

            if (backgroundServiceInfo.LastHeartbeat == null)
                return true;

            var elapsed = now - backgroundServiceInfo.LastHeartbeat.Value;

            if (elapsed > TimeSpan.FromSeconds(HearthBeatInterval.TotalSeconds * 5))
            {
                return true;
            }
        }

        return false;
    }

    protected virtual async Task StartBackgroundServiceInfoAsync(CancellationToken cancellationToken)
    {
        if (BackgroundServiceInfoEnabled == false)
            return;

        using var scope = _serviceScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<IDbContext>();
        var DbContextTransaction = scope.ServiceProvider.GetService<DbContextTransaction>();
        var now = _dateTimeService.Now();

        var backgroundServiceInfo = await GetBackgroundServiceInfoAsync(context);

        if (backgroundServiceInfo == null)
        {
            backgroundServiceInfo = await context.CreateEntityAync<BackgroundserviceInfo>();
            backgroundServiceInfo.Name = Name;
        }

        backgroundServiceInfo.State = BackgroundserviceInfoStateType.Executing;
        backgroundServiceInfo.ExecutedAt = now;
        backgroundServiceInfo.NextExecuteAt = null;
        backgroundServiceInfo.Message = $"executing";
        backgroundServiceInfo.LastHeartbeat = now;

        await context.SaveChangesAsync(cancellationToken);
    }

    protected virtual async Task FinishedBackgroundServiceInfoAsync(CancellationToken cancellationToken)
    {
        if (BackgroundServiceInfoEnabled == false)
            return;

        using var scope = _serviceScopeFactory.CreateScope();
        var now = _dateTimeService.Now();

        var context = scope.ServiceProvider.GetService<IDbContext>();

        var backgroundServiceInfo = await GetBackgroundServiceInfoAsync(context);

        backgroundServiceInfo.State = BackgroundserviceInfoStateType.Queued;
        backgroundServiceInfo.LastFinishedAt = now;
        backgroundServiceInfo.LastHeartbeat = null;

        backgroundServiceInfo.NextExecuteAt =
            CalcNextExecuteAt(now, backgroundServiceInfo);

        backgroundServiceInfo.Message = $"finished";

        await context.SaveChangesAsync(cancellationToken);
    }

    protected virtual async Task UpdateHeartbeatBackgroundServiceInfoAsync(CancellationToken cancellationToken)
    {
        if (BackgroundServiceInfoEnabled == false)
            return;

        using var scope = _serviceScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<IDbContext>();

        var backgroundServiceInfo = await GetBackgroundServiceInfoAsync(context);

        backgroundServiceInfo.LastHeartbeat = _dateTimeService.Now();

        await context.SaveChangesAsync(cancellationToken);
    }

    protected virtual async Task ErrorBackgroundServiceInfoAsync(Exception ex, CancellationToken cancellationToken)
    {
        if (BackgroundServiceInfoEnabled == false)
            return;

        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<IDbContext>();

        var now = _dateTimeService.Now();

        var backgroundServiceInfo = await GetBackgroundServiceInfoAsync(context);

        backgroundServiceInfo.State = BackgroundserviceInfoStateType.Error;
        backgroundServiceInfo.Message = $"error";
        backgroundServiceInfo.LastFinishedAt = null;
        backgroundServiceInfo.LastErrorAt = now;
        backgroundServiceInfo.LastErrorMessage = ex.Message;
        backgroundServiceInfo.LastErrorStack = ex.StackTrace;
        backgroundServiceInfo.LastHeartbeat = null;

        if (_configuration.Interval.HasValue)
        {
            backgroundServiceInfo.NextExecuteAt =
                CalcNextExecuteAt(now, backgroundServiceInfo);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private IEnumerable<DateRange> GetEnabledDateRange(DateTime dateTime)
    {
        if (_configuration.EnabledFromTime.HasValue &&
            _configuration.EnabledToTime.HasValue)
        {
            var enabledFromTime = _configuration.EnabledFromTime.Value;
            var enabledToTime = _configuration.EnabledToTime.Value;
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

        if (_configuration.Interval.IsNull())
            return dateTime;

        var nextExecuteAt = dateTime.Add(_configuration.Interval.Value);

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

    protected async Task<BackgroundserviceInfo> GetBackgroundServiceInfoAsync(IDbContext context)
    {
        return await
            context.Set<BackgroundserviceInfo>()
            .FirstOrDefaultAsync(_ => _.Name == Name);
    }
}