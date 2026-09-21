using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.DependencyInjection;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.SqlServer;

[TransientDependency<IDistributedLock>]
public sealed class SQLServerDistributedLock : IDistributedLock
{
    private const int _lockTimeout = 180000;
    private readonly string _connectionString;
    private SqlConnection _connection;
    private SqlTransaction _transaction;
    private bool _disposed;
    private int _operationInProgress;

    public SQLServerDistributedLock(IDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var connectionString = context.Database.GetConnectionString();
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
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

        if (lockId.Length > 255)
            throw new ArgumentException("SQL application lock names must not exceed 255 characters.", nameof(lockId));

        if (_transaction is not null)
            throw new InvalidOperationException("This instance already holds a SQL application lock.");

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (await TryCreateLockAsync(lockId, cancellationToken))
                    return true;
            }
            catch (Exception acquisitionException)
            {
                try
                {
                    await ReleaseLockAsync();
                }
                catch (Exception cleanupException)
                {
                    throw new AggregateException("SQL application lock acquisition and cleanup failed.", acquisitionException, cleanupException);
                }

                throw;
            }

            await ReleaseLockAsync();

            if (retry == 0)
                return false;

            retry--;
            await Task.Delay(100, cancellationToken);
        }
    }

    private async Task<bool> TryCreateLockAsync(string lockId, CancellationToken cancellationToken)
    {
        _connection = new SqlConnection(_connectionString);
        await _connection.OpenAsync(cancellationToken);
        _transaction = (SqlTransaction)await _connection.BeginTransactionAsync(cancellationToken);

        await using var command = _connection.CreateCommand();
        command.Transaction = _transaction;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "sys.sp_getapplock";
        command.CommandTimeout = (int)Math.Ceiling(_lockTimeout / 1000d) + 30;
        command.Parameters.Add("@Resource", SqlDbType.NVarChar, 255).Value = lockId;
        command.Parameters.Add("@LockMode", SqlDbType.VarChar, 32).Value = "Exclusive";
        command.Parameters.Add("@LockOwner", SqlDbType.VarChar, 32).Value = "Transaction";
        command.Parameters.Add("@LockTimeout", SqlDbType.Int).Value = _lockTimeout;
        command.Parameters.Add("@DbPrincipal", SqlDbType.NVarChar, 128).Value = "public";
        var returnValue = command.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
        returnValue.Direction = ParameterDirection.ReturnValue;

        await command.ExecuteNonQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        return returnValue.Value switch
        {
            0 or 1 => true,
            -1 => false,
            _ => throw new InvalidOperationException($"Unable to acquire SQL application lock '{lockId}' (result: {returnValue.Value}).")
        };
    }

    private void EnterOperation()
    {
        if (Interlocked.CompareExchange(ref _operationInProgress, 1, 0) != 0)
            throw new InvalidOperationException("Concurrent operations on a SQLServerDistributedLock instance are not supported.");

        if (_disposed)
        {
            Volatile.Write(ref _operationInProgress, 0);
            throw new ObjectDisposedException(nameof(SQLServerDistributedLock));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _operationInProgress, 1, 0) != 0)
            throw new InvalidOperationException("Cannot dispose a SQLServerDistributedLock instance while an operation is running.");

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
            // Ending the dedicated transaction releases the lock, even during cancellation.
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