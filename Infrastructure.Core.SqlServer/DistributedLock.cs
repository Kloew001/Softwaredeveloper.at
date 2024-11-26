using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoftwaredeveloperDotAt.Infrastructure.Core.Utility;
using System.Data;
using System.Text;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

[TransientDependency<IDistributedLock>]
public sealed class SQLServerDistributedLock : IDistributedLock, IDisposable
{
    private const string _lockMode = "Exclusive";
    private const string _lockOwner = "Transaction";
    private const string _lockDbPrincipal = "public";

    private string _lockId;

    private int _lockTimeout = 180000;

    private SqlConnection _connection;
    private SqlTransaction _transaction;

    private bool _lockCreated = false;
    private readonly ILogger<SQLServerDistributedLock> _logger;

    public SQLServerDistributedLock(IDbContext context, ILogger<SQLServerDistributedLock> logger)
    {
        _logger = logger;

        var connectionString = context.Database.GetConnectionString();
        _connection = new SqlConnection(connectionString);
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

    public async Task<bool> TryAcquireLockAsync(string lockId, int retry = 0)
    {
        _lockId = lockId;

        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();

        _transaction = (SqlTransaction)await _connection.BeginTransactionAsync();
        
        using (SqlCommand createCmd = _connection.CreateCommand())
        {
            createCmd.Transaction = _transaction;
            createCmd.CommandTimeout = _lockTimeout;
            createCmd.CommandType = System.Data.CommandType.Text;

            StringBuilder sbCreateCommand = new StringBuilder();
            sbCreateCommand.AppendLine("DECLARE @res INT");
            sbCreateCommand.AppendLine("EXEC @res = sp_getapplock");
            sbCreateCommand.Append("@Resource = '").Append(_lockId).AppendLine("',");
            sbCreateCommand.Append("@LockMode = '").Append(_lockMode).AppendLine("',");
            sbCreateCommand.Append("@LockOwner = '").Append(_lockOwner).AppendLine("',");
            sbCreateCommand.Append("@LockTimeout = ").Append(_lockTimeout).AppendLine(",");
            sbCreateCommand.Append("@DbPrincipal = '").Append(_lockDbPrincipal).AppendLine("'");
            sbCreateCommand.AppendLine("IF @res NOT IN (0, 1)");
            sbCreateCommand.AppendLine("BEGIN");
            sbCreateCommand.AppendLine("RAISERROR ( 'Unable to acquire Lock', 16, 1 )");
            sbCreateCommand.AppendLine("END");

            createCmd.CommandText = sbCreateCommand.ToString();

            try
            {
                await createCmd.ExecuteNonQueryAsync();

                _lockCreated = true;
            }
            catch (Exception ex)
            {
                if (_transaction != null &&
                    _transaction.Connection != null &&
                    _transaction.Connection.State == System.Data.ConnectionState.Open)
                    _transaction.Rollback();

                throw new Exception(string.Format("Unable to get SQL Application Lock on '{0}'", _lockId), ex);
            }
        }

        if (_lockCreated == false)
        {
            for (var r = 0; r < retry; r++)
            {
                await Task.Delay(100);

                if (await TryAcquireLockAsync(lockId, 0))
                    return true;
            }
        }

        return _lockCreated;
    }
    
    public bool TryAcquireLock(string lockId, int retry = 0)
    {
        return TryAcquireLockAsync(lockId, retry).GetAwaiter().GetResult();
    }

    private void ReleaseLock()
    {
        using (SqlCommand releaseCmd = _connection.CreateCommand())
        {
            releaseCmd.Transaction = _transaction;
            releaseCmd.CommandType = System.Data.CommandType.StoredProcedure;
            releaseCmd.CommandText = "sp_releaseapplock";

            releaseCmd.Parameters.AddWithValue("@Resource", _lockId);
            releaseCmd.Parameters.AddWithValue("@LockOwner", _lockOwner);
            releaseCmd.Parameters.AddWithValue("@DbPrincipal", _lockDbPrincipal);

            try
            {
                releaseCmd.ExecuteNonQuery();
                _transaction.Commit();
            }
            catch (Exception)
            {
            }
        }
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed == true)
        {
        }

        if (_disposed == false)
        {
            if (_lockCreated)
            {
                ReleaseLock();
            }

            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }
        _disposed = true;
    }
}