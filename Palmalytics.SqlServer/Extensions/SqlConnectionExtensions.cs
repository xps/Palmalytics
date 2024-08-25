using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Palmalytics.SqlServer.Locking;

namespace Palmalytics.SqlServer.Extensions
{
    internal static class SqlConnectionExtensions
    {
        public static SqlLock GetAppLock(this SqlConnection connection, string resource, int? millisecondsTimeout = null)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("The connection must be in Open state, or else this method will open and close the connection, and the lock will be released immediately.");

            var parameters = new DynamicParameters();
            parameters.Add("@Resource", resource);
            parameters.Add("@LockMode", "Exclusive");
            parameters.Add("@LockOwner", "Session");

            if (millisecondsTimeout != null)
                parameters.Add("@LockTimeout", millisecondsTimeout);

            parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            connection.Execute("sp_getapplock", parameters, commandType: CommandType.StoredProcedure);

            var result = parameters.Get<int>("@ReturnValue");
            if (result >= 0)
            {
                // Lock was granted
                return new SqlLock(connection, resource);
            }
            else
            {
                throw new CouldNotAcquireLockException($"Could not get lock for resource '{resource}': sp_getapplock returned value: {result}.");
            }
        }

        public static async Task<SqlLock> GetAppLockAsync(this SqlConnection connection, string resource, int? millisecondsTimeout = null)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("The connection must be in Open state, or else this method will open and close the connection, and the lock will be released immediately.");

            var parameters = new DynamicParameters();
            parameters.Add("@Resource", resource);
            parameters.Add("@LockMode", "Exclusive");
            parameters.Add("@LockOwner", "Session");

            if (millisecondsTimeout != null)
                parameters.Add("@LockTimeout", millisecondsTimeout);

            parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            await connection.ExecuteAsync("sp_getapplock", parameters, commandType: CommandType.StoredProcedure);

            var result = parameters.Get<int>("@ReturnValue");
            if (result >= 0)
            {
                // Lock was granted
                return new SqlLock(connection, resource);
            }
            else
            {
                throw new CouldNotAcquireLockException($"Could not get lock for resource '{resource}': sp_getapplock returned value: {result}.");
            }
        }

        public static void ReleaseAppLock(this SqlConnection connection, string resource)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("The connection must be in Open state, because no lock can be held by a closed connection.");

            connection.Execute(
                "sp_releaseapplock",
                new { Resource = resource, LockOwner = "Session" },
                commandType: CommandType.StoredProcedure
            );
        }

        public static async Task ReleaseAppLockAsync(this SqlConnection connection, string resource)
        {
            if (connection.State != ConnectionState.Open)
                throw new InvalidOperationException("The connection must be in Open state, because no lock can be held by a closed connection.");

            await connection.ExecuteAsync(
                "sp_releaseapplock",
                new { Resource = resource, LockOwner = "Session" },
                commandType: CommandType.StoredProcedure
            );
        }

        private static SqlCommand CreateCommand(this SqlConnection connection, string sql, Action<SqlCommand> setup)
        {
            var command = new SqlCommand(sql, connection);
            setup?.Invoke(command);
            return command;
        }
    }
}
