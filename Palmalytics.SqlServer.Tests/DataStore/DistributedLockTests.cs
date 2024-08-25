using System.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Palmalytics.SqlServer.Extensions;
using Palmalytics.SqlServer.Locking;

namespace Palmalytics.SqlServer.Tests.DataStore
{
    public class DistributedLockTests
    {
        [Fact]
        public async Task Test_DistributedLock_Prevents_Locking_Twice()
        {
            // Arrange
            await using var connection1 = await OpenConnectionAsync();
            await using var connection2 = await OpenConnectionAsync();

            // Act
            using var @lock = connection1.GetAppLock("x");

            // Assert
            var lock2 = await connection2.Awaiting(x => x.GetAppLockAsync("x", 0))
                .Should().ThrowAsync<CouldNotAcquireLockException>();
        }

        [Fact]
        public async Task Test_DistributedLock_Prevents_Locking_Twice_Async()
        {
            // Arrange
            await using var connection1 = await OpenConnectionAsync();
            await using var connection2 = await OpenConnectionAsync();

            // Act
            await using var @lock = await connection1.GetAppLockAsync("x");

            // Assert
            var lock2 = await connection2.Awaiting(x => x.GetAppLockAsync("x", 0))
                .Should().ThrowAsync<CouldNotAcquireLockException>();
        }

        [Fact]
        public async Task Test_DistributedLock_Can_Lock_Again_After_Release()
        {
            // Arrange
            await using var connection1 = await OpenConnectionAsync();
            await using var connection2 = await OpenConnectionAsync();

            // Act
            using (var @lock = connection1.GetAppLock("x"))
            {
            }

            // Assert - test passes if we can get through this lock
            using (var @lock = connection2.GetAppLock("x"))
            {
            }
        }

        [Fact]
        public async Task Test_DistributedLock_Can_Lock_Again_After_Release_Async()
        {
            // Arrange
            await using var connection1 = await OpenConnectionAsync();
            await using var connection2 = await OpenConnectionAsync();

            // Act
            await using (var @lock = await connection1.GetAppLockAsync("x"))
            {
            }

            // Assert - test passes if we can get through this lock
            await using (var @lock = await connection2.GetAppLockAsync("x"))
            {
            }
        }

        private static async Task<SqlConnection> OpenConnectionAsync()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", optional: false)
                .Build();

            // Check config
            var options = configuration.Get<TestOptions>();
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new ConfigurationErrorsException("ConnectionString must be set in appsettings.json");

            var connection = new SqlConnection(options.ConnectionString);

            await connection.OpenAsync();

            return connection;
        }
    }
}