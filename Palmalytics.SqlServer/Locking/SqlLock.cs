using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Palmalytics.SqlServer.Extensions;

namespace Palmalytics.SqlServer.Locking
{
    public class SqlLock(SqlConnection connection, string resource) : IDisposable, IAsyncDisposable
    {
        public void Dispose() => connection.ReleaseAppLock(resource);
        public async ValueTask DisposeAsync() => await connection.ReleaseAppLockAsync(resource);
    }
}
