using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    /// <summary>
    /// https://ankitvijay.net/2021/02/28/distributed-lock-using-postgresql/comment-page-1/
    /// select pg_try_advisory_lock(hashtext('TEST')) as "lockObtained";
    /// select pg_advisory_unlock(hashtext('TEST')) as "lockReleased";
    /// select* from pg_locks join pg_stat_activity using (pid) where locktype='advisory';
    /// </summary>
    public sealed class PostgreSQLDistributedLock : IDistributedLock, ITransientService, ITypedTransientService<IDistributedLock>, IDisposable
    {
        private string _lockId;
        private bool _disposed;
        private NpgsqlConnection _connection;
        private readonly ILogger<PostgreSQLDistributedLock> _logger;

        public PostgreSQLDistributedLock(IDbContext context, ILogger<PostgreSQLDistributedLock> logger)
        {
            _logger = logger;
            var builder = new NpgsqlConnectionStringBuilder(context.Database.GetConnectionString());
            _connection = new NpgsqlConnection(builder.ToString());
            _connection.Open();
        }

        public bool TryExecuteInDistributedLock(string lockId, Func<Task> exclusiveLockTask)
        {
            var hasLockedAcquired = TryAcquireLock(lockId);

            if (!hasLockedAcquired)
            {
                return false;
            }

            try
            {
                exclusiveLockTask();
            }
            finally
            {
                ReleaseLock();
            }

            return true;
        }

        public bool TryAcquireLock(string lockId, int retry = 0)
        {
            _lockId = lockId;

            var sessionLockCommand = $"SELECT pg_try_advisory_lock(hashtext('{lockId}'))";
            _logger.LogInformation("Trying to acquire session lock for Lock Id {@LockId}", lockId);
            var commandQuery = new NpgsqlCommand(sessionLockCommand, _connection);
            var result = commandQuery.ExecuteScalar();
            if (result != null && bool.TryParse(result.ToString(), out var lockAcquired) && lockAcquired)
            {
                _logger.LogInformation("Lock {@LockId} acquired", lockId);
                return true;
            }

            _logger.LogInformation("Lock {@LockId} rejected", lockId);

            for (var r = 0; r < retry; r++)
            {
                Thread.Sleep(50);

                if (TryAcquireLock(lockId, 0))
                    return true;
            }

            return false;
        }

        private void ReleaseLock()
        {
            var transactionLockCommand = $"SELECT pg_advisory_unlock(hashtext('{_lockId}'))";
            _logger.LogInformation("Releasing session lock for {@LockId}", _lockId);
            var commandQuery = new NpgsqlCommand(transactionLockCommand, _connection);
            commandQuery.ExecuteScalar();
        }

        public void Dispose()
        {
            if (_disposed == false)
            {
                ReleaseLock();

                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }

            _disposed = true;
        }
    }
}