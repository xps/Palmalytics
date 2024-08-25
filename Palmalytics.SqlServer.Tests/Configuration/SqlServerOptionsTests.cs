using Palmalytics.Exceptions;

namespace Palmalytics.SqlServer.Tests.Configuration
{
    public class SqlServerOptionsTests
    {
        [Fact]
        public void UseSqlServer_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var options = new PalmalyticsOptions();

            // Act
            Action act = () => options.UseSqlServer(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sqlServerOptions");
        }

        [Fact]
        public void UseSqlServer_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Arrange
            var options = new PalmalyticsOptions();
            var sqlServerOptions = new SqlServerOptions { ConnectionString = null };

            // Act
            Action act = () => options.UseSqlServer(sqlServerOptions);

            // Assert
            act.Should().Throw<PalmalyticsOptionsException>().WithMessage("ConnectionString cannot be null or empty");
        }
    }
}
