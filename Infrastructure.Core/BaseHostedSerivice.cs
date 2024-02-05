using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
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

        public string Message { get; set; }

        public DateTime? LastErrorAt { get; set; }
        public string LastErrorMessage { get; set; }
        public string LastErrorStack { get; set; }
    }

    public abstract class BaseHostedSerivice : IHostedService, IDisposable
    {
        protected readonly IServiceScopeFactory _serviceScopeFactory;
        protected readonly ILogger<BaseHostedSerivice> _logger;
        protected readonly IApplicationSettings _applicationSettings;
        protected readonly HostedServicesConfiguration _hostedServicesConfiguration;
        protected readonly IHostApplicationLifetime _appLifetime;
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public virtual string Name { get => GetType().Name; }

        protected BaseHostedSerivice(
            IServiceScopeFactory serviceScopeFactory,
            IHostApplicationLifetime appLifetime,
            ILogger<BaseHostedSerivice> logger,
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

            if (_hostedServicesConfiguration == null ||
            _hostedServicesConfiguration?.Enabled != true)
            {
                _logger.LogWarning($"IHostedService {Name} do not have configuration");
                return Task.CompletedTask;
            }

            _appLifetime.ApplicationStarted.Register(async () =>
                await ExecuteAsync(cancellationToken));

            return Task.CompletedTask;
        }

        protected virtual async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"IHostedService execute for {Name}");

                try
                {
                    await CreateOrUpdateBackgroundServiceInfo();

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

        protected virtual async Task CreateOrUpdateBackgroundServiceInfo()
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
                    backgroundServiceInfo = context.CreateProxy<BackgroundserviceInfo>();
                    await context.AddAsync(backgroundServiceInfo);
                    backgroundServiceInfo.Name = this.Name;
                }

                backgroundServiceInfo.ExecutedAt = DateTime.Now;
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
                backgroundServiceInfo.Message = $"finished";

                await context.SaveChangesAsync();
            }
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

                await context.SaveChangesAsync();
            }
        }

        private async Task<BackgroundserviceInfo> GetBackgroundServiceInfo(IDbContext context)
        {
            return await
                context.Set<BackgroundserviceInfo>()
                .SingleOrDefaultAsync(_ => _.Name == this.Name);
        }
    }
}
