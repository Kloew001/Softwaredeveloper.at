using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public interface IDistributedLockService : IAsyncDisposable
{
    Task<bool> TryAcquireLockAsync(
        string lockId,
        int retry = 3,
        CancellationToken cancellationToken = default,
        TimeSpan? leaseDuration = null,
        TimeSpan? renewInterval = null);

    CancellationToken LockLost { get; }
}

[Table(nameof(DistributedLock), Schema = "core")]
public class DistributedLock
{
    [Key]
    [Required]
    [MaxLength(400)]
    public string LockId { get; set; }

    [Required]
    [MaxLength(64)]
    public string Owner { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }
}

public class DistributedLockConfiguration : IEntityTypeConfiguration<DistributedLock>
{
    public void Configure(EntityTypeBuilder<DistributedLock> builder)
    {
        builder.HasKey(_ => _.LockId);
        builder.HasIndex(_ => _.ExpiresAt);
    }
}

[TransientDependency<IDistributedLockService>]
public sealed class DistributedLockService : IDistributedLockService
{
    public static readonly TimeSpan DefaultLeaseDuration = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan DefaultRenewInterval = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DistributedLockService> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly string _owner = Guid.NewGuid().ToString("N");

    private string _lockId;
    private TimeSpan _leaseDuration;
    private TimeSpan _renewInterval;
    private CancellationTokenSource _renewCts;
    private Task _renewTask;
    private bool _disposed;
    private int _operationInProgress;
    private readonly CancellationTokenSource _lockLostCts = new();

    public CancellationToken LockLost => _lockLostCts.Token;

    public DistributedLockService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DistributedLockService> logger,
        IDateTimeService dateTimeService)
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dateTimeService);

        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<bool> TryAcquireLockAsync(
        string lockId,
        int retry = 3,
        CancellationToken cancellationToken = default,
        TimeSpan? leaseDuration = null,
        TimeSpan? renewInterval = null)
    {
        EnterOperation();

        try
        {
            return await TryAcquireLockCoreAsync(lockId, retry, cancellationToken, leaseDuration, renewInterval);
        }
        finally
        {
            Volatile.Write(ref _operationInProgress, 0);
        }
    }

    private async Task<bool> TryAcquireLockCoreAsync(
        string lockId,
        int retry,
        CancellationToken cancellationToken,
        TimeSpan? leaseDuration,
        TimeSpan? renewInterval)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lockId);
        ArgumentOutOfRangeException.ThrowIfNegative(retry);

        if (_lockId is not null)
            throw new InvalidOperationException("This instance already holds a distributed lock.");

        _leaseDuration = leaseDuration ?? DefaultLeaseDuration;
        _renewInterval = renewInterval ?? DefaultRenewInterval;

        if (_leaseDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration), "Lease duration must be positive.");

        if (_renewInterval <= TimeSpan.Zero || _renewInterval >= _leaseDuration)
            throw new ArgumentOutOfRangeException(nameof(renewInterval), "Renew interval must be positive and smaller than the lease duration.");

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogDebug("Trying to acquire lease lock for Lock Id {@LockId}", lockId);

            if (await TryClaimLeaseAsync(lockId, cancellationToken))
            {
                _lockId = lockId;
                StartRenewal();

                _logger.LogDebug("Lock {@LockId} acquired", lockId);
                return true;
            }

            _logger.LogDebug("Lock {@LockId} rejected", lockId);

            if (retry == 0)
                return false;

            retry--;
            await Task.Delay(100, cancellationToken);
        }
    }

    private async Task<bool> TryClaimLeaseAsync(string lockId, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

        var now = _dateTimeService.Now();
        var expiresAt = now.Add(_leaseDuration);
        var owner = _owner;

        var updated = await context.Set<DistributedLock>()
            .Where(_ => _.LockId == lockId && (_.ExpiresAt < now || _.Owner == owner))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(_ => _.Owner, owner)
                .SetProperty(_ => _.ExpiresAt, expiresAt),
                cancellationToken);

        if (updated > 0)
            return true;

        try
        {
            context.Add(new DistributedLock
            {
                LockId = lockId,
                Owner = owner,
                ExpiresAt = expiresAt
            });

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    private void StartRenewal()
    {
        _renewCts = new CancellationTokenSource();
        var token = _renewCts.Token;
        var lockId = _lockId;
        var renewInterval = _renewInterval;

        _renewTask = Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested && !_lockLostCts.IsCancellationRequested)
                {
                    await Task.Delay(renewInterval.GetJitteredInterval(), token);
                    await RenewLeaseAsync(lockId, token);
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                //ignore, lock is being released
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to renew distributed lock {@LockId}", lockId);
                _lockLostCts.Cancel();
            }
        });
    }

    private async Task RenewLeaseAsync(string lockId, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

        var expiresAt = _dateTimeService.Now().Add(_leaseDuration);
        var owner = _owner;

        var updated = await context.Set<DistributedLock>()
            .Where(_ => _.LockId == lockId && _.Owner == owner)
            .ExecuteUpdateAsync(setters => setters.SetProperty(_ => _.ExpiresAt, expiresAt), cancellationToken);

        if (updated == 0)
        {
            _logger.LogWarning("Lost ownership of distributed lock {@LockId} while renewing its lease.", lockId);
            _lockLostCts.Cancel();
        }
    }

    private void EnterOperation()
    {
        if (Interlocked.CompareExchange(ref _operationInProgress, 1, 0) != 0)
            throw new InvalidOperationException("Concurrent operations on a DistributedLockService instance are not supported.");

        if (_disposed)
        {
            Volatile.Write(ref _operationInProgress, 0);
            throw new ObjectDisposedException(nameof(DistributedLockService));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _operationInProgress, 1, 0) != 0)
            throw new InvalidOperationException("Cannot dispose a DistributedLockService instance while an operation is running.");

        try
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                await ReleaseLockAsync();
            }
            finally
            {
                _lockLostCts.Cancel();
                _lockLostCts.Dispose();
            }
        }
        finally
        {
            Volatile.Write(ref _operationInProgress, 0);
        }
    }

    private async Task ReleaseLockAsync()
    {
        if (_renewCts is not null)
        {
            _renewCts.Cancel();

            try
            {
                await _renewTask;
            }
            catch
            {
                //ignore, renewal loop observed cancellation or already logged its own failure
            }

            _renewCts.Dispose();
            _renewCts = null;
            _renewTask = null;
        }

        var lockId = _lockId;
        _lockId = null;

        if (lockId is null)
            return;

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IDbContext>();

            var owner = _owner;

            await context.Set<DistributedLock>()
                .Where(_ => _.LockId == lockId && _.Owner == owner)
                .ExecuteDeleteAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Not fatal: the lease will simply expire on its own after LeaseDuration.
            _logger.LogWarning(ex, "Failed to explicitly release distributed lock {@LockId}; it will expire via lease timeout.", lockId);
        }
    }
}
