using System.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Npgsql;
using NpgsqlTypes;

using SoftwaredeveloperDotAt.Infrastructure.Core.DependencyInjection;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.PostgreSQL;

/// <summary>
/// Holds a PostgreSQL advisory lock in a dedicated transaction until asynchronous disposal.
/// </summary>
[TransientDependency<IDistributedLock>]
public sealed class PostgreSQLDistributedLock : IDistributedLock
{
    private readonly string _connectionString;
    private readonly ILogger<PostgreSQLDistributedLock> _logger;
    private NpgsqlConnection _connection;
    private NpgsqlTransaction _transaction;
    private bool _disposed;
    private int _operationInProgress;

    public PostgreSQLDistributedLock(IDbContext context, ILogger<PostgreSQLDistributedLock> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);
        var connectionString = context.Database.GetConnectionString();
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // The lock transaction must be independent of any ambient TransactionScope.
        var builder = new NpgsqlConnectionStringBuilder(connectionString) { Enlist = false };
        _connectionString = builder.ConnectionString;
        _logger = logger;
    }

    public async Task<bool> TryAcquireLockAsync(string lockId, int retry = 3, CancellationToken cancellationToken = default)
    {
        EnterOperation();

        try
        {
            return await TryAcquireLockCoreAsync(lockId, retry, cancellationToken);
        }
        finally
        {
            Volatile.Write(ref _operationInProgress, 0);
        }
    }

    private async Task<bool> TryAcquireLockCoreAsync(string lockId, int retry, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lockId);
        ArgumentOutOfRangeException.ThrowIfNegative(retry);

        if (_transaction is not null)
            throw new InvalidOperationException("This instance already holds a PostgreSQL advisory lock.");

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _logger.LogDebug("Trying to acquire transaction lock for Lock Id {@LockId}", lockId);

                if (await TryCreateLockAsync(lockId, cancellationToken))
                {
                    _logger.LogDebug("Lock {@LockId} acquired", lockId);
                    return true;
                }
            }
            catch (Exception acquisitionException)
            {
                try
                {
                    await ReleaseLockAsync();
                }
                catch (Exception cleanupException)
                {
                    throw new AggregateException("PostgreSQL advisory lock acquisition and cleanup failed.", acquisitionException, cleanupException);
                }

                throw;
            }

            await ReleaseLockAsync();
            _logger.LogDebug("Lock {@LockId} rejected", lockId);

            if (retry == 0)
                return false;

            retry--;
            await Task.Delay(100, cancellationToken);
        }
    }

    private async Task<bool> TryCreateLockAsync(string lockId, CancellationToken cancellationToken)
    {
        _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync(cancellationToken);
        _transaction = await _connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        await using var command = _connection.CreateCommand();
        command.Transaction = _transaction;
        // Preserve the existing key and bigint lock namespace for mixed-version deployments.
        command.CommandText = "SELECT pg_catalog.pg_try_advisory_xact_lock(pg_catalog.hashtext(@lockId)::bigint)";
        command.Parameters.Add("lockId", NpgsqlDbType.Text).Value = lockId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        return result is bool acquired
            ? acquired
            : throw new InvalidOperationException("PostgreSQL advisory lock acquisition returned an unexpected result.");
    }

    private void EnterOperation()
    {
        if (Interlocked.CompareExchange(ref _operationInProgress, 1, 0) != 0)
            throw new InvalidOperationException("Concurrent operations on a PostgreSQLDistributedLock instance are not supported.");

        if (_disposed)
        {
            Volatile.Write(ref _operationInProgress, 0);
            throw new ObjectDisposedException(nameof(PostgreSQLDistributedLock));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _operationInProgress, 1, 0) != 0)
            throw new InvalidOperationException("Cannot dispose a PostgreSQLDistributedLock instance while an operation is running.");

        try
        {
            if (_disposed)
                return;

            _disposed = true;
            await ReleaseLockAsync();
        }
        finally
        {
            Volatile.Write(ref _operationInProgress, 0);
        }
    }

    private async Task ReleaseLockAsync()
    {
        var transaction = _transaction;
        var connection = _connection;
        _transaction = null;
        _connection = null;

        try
        {
            // Rollback releases the lock even if acquisition was canceled after reaching the server.
            if (transaction?.Connection is not null)
                await transaction.RollbackAsync(CancellationToken.None);
        }
        finally
        {
            try
            {
                if (transaction is not null)
                    await transaction.DisposeAsync();
            }
            finally
            {
                if (connection is not null)
                    await connection.DisposeAsync();
            }
        }
    }
}