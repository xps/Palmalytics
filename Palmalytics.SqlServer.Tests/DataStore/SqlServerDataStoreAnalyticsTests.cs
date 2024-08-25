using Palmalytics.Model;
using static Palmalytics.SqlServer.Tests.DataStoreHelpers;

namespace Palmalytics.SqlServer.Tests.DataStore
{
    public class SqlServerDataStoreAnalyticsTests : IClassFixture<PersistentDataSetFixture>
    {
        [Fact]
        public void Test_SqlServerDataStore_GetTopData_Works_With_No_Data()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();

            // Act
            var data = dataStore.GetTopData(null, null, null);

            // Assert
            data.TotalSessions.Should().Be(0);
            data.TotalPageViews.Should().Be(0);
            data.AverageBounceRate.Should().Be(0);
            data.AverageSessionDuration.Should().Be(0);
            data.AveragePagesPerSession.Should().Be(0);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetTopData_Works_With_No_Filter()
        {
            // Arrange
            var dataStore = CreatePersistentDataStore();

            // Act
            var data = dataStore.GetTopData(null, null, null);

            // Assert
            data.TotalSessions.Should().Be(1_810);
            data.TotalPageViews.Should().Be(3_620);
            data.AverageBounceRate.Should().Be(100 * 3 / 10);
            data.AverageSessionDuration.Should().Be(10);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetTopData_Works_With_Filters()
        {
            // Arrange
            var dataStore = CreatePersistentDataStore();

            // Act
            var data = dataStore.GetTopData(
                DateTime.Parse("2023-01-01"),
                DateTime.Parse("2023-01-31"),
                null
            );

            // Assert
            data.TotalSessions.Should().Be(310);
            data.TotalPageViews.Should().Be(620);
            data.AverageBounceRate.Should().Be(100 * 3 / 10);
            data.AverageSessionDuration.Should().Be(10);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetChart_Works_With_No_Data()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();

            // Act
            var data = dataStore.GetChart(null, null, Interval.Months, ChartProperty.Sessions, null);

            // Assert
            data.Data.Should().HaveCount(0);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetChart_Works_With_No_Filter()
        {
            // Arrange
            var dataStore = CreatePersistentDataStore();

            // Act
            var data = dataStore.GetChart(null, null, Interval.Months, ChartProperty.Sessions, null);

            // Assert
            data.Data.Should().HaveCount(6);
            data.Data.Sum(x => x.Value).Should().Be(1810);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetChart_Works_With_Filters()
        {
            // Arrange
            var dataStore = CreatePersistentDataStore();

            // Act
            var data = dataStore.GetChart(
                DateTime.Parse("2023-01-01"),
                DateTime.Parse("2023-01-31"),
                Interval.Days,
                ChartProperty.Sessions,
                null
            );

            // Assert
            data.Data.Should().HaveCount(31);
            data.Data.Sum(x => x.Value).Should().Be(310);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetChart_Can_Return_PageViews()
        {
            // Arrange
            var dataStore = CreatePersistentDataStore();

            // Act
            var data = dataStore.GetChart(null, null, Interval.Months, ChartProperty.PageViews, null);

            // Assert
            data.Data.Should().HaveCount(6);
            data.Data.Sum(x => x.Value).Should().Be(3620);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetChart_Can_Return_BounceRate()
        {
            // Arrange
            var dataStore = CreatePersistentDataStore();

            // Act
            var data = dataStore.GetChart(null, null, Interval.Months, ChartProperty.BounceRate, null);

            // Assert
            data.Data.Should().HaveCount(6);
            data.Data.Average(x => x.Value).Should().Be(30);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetChart_Can_Return_SessionDuration()
        {
            // Arrange
            var dataStore = CreatePersistentDataStore();

            // Act
            var data = dataStore.GetChart(null, null, Interval.Months, ChartProperty.SessionDuration, null);

            // Assert
            data.Data.Should().HaveCount(6);
            data.Data.Average(x => x.Value).Should().Be(10);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetBrowsers_Works_With_No_Data()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();

            // Act
            var data = dataStore.GetBrowsers(null, null, null, 1);

            // Assert
            data.TotalRows.Should().Be(0);
            data.PageCount.Should().Be(0);
            data.Rows.Should().HaveCount(0);
            data.SamplingFactor.Should().Be(1);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetBrowsers_Works_With_No_Filter()
        {
            // Arrange
            var dataStore = CreatePersistentDataStore();

            // Act
            var data = dataStore.GetBrowsers(null, null, null, 1);

            // Assert
            data.TotalRows.Should().Be(3);
            data.PageCount.Should().Be(1);
            data.Rows.Should().HaveCount(3);
            data.Rows[0].Label.Should().Be("Chrome");
            data.Rows[0].Percentage.Should().Be(50);
            data.Rows[0].Value.Should().Be(905);
            data.Rows[1].Label.Should().Be("Safari");
            data.Rows[1].Percentage.Should().Be(30);
            data.Rows[1].Value.Should().Be(543);
            data.Rows[2].Label.Should().Be("Firefox");
            data.Rows[2].Percentage.Should().Be(20);
            data.Rows[2].Value.Should().Be(362);
            data.SamplingFactor.Should().Be(1);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetBrowsers_Works_With_Filters()
        {
            // Arrange
            var dataStore = CreatePersistentDataStore();

            // Act
            var data = dataStore.GetBrowsers(DateTime.Parse("2023-01-01"), DateTime.Parse("2023-01-31"), null, 1);

            // Assert
            data.TotalRows.Should().Be(3);
            data.PageCount.Should().Be(1);
            data.Rows[0].Label.Should().Be("Chrome");
            data.Rows[0].Percentage.Should().Be(50);
            data.Rows[0].Value.Should().Be(155);
            data.Rows[1].Label.Should().Be("Safari");
            data.Rows[1].Percentage.Should().Be(30);
            data.Rows[1].Value.Should().Be(93);
            data.Rows[2].Label.Should().Be("Firefox");
            data.Rows[2].Percentage.Should().Be(20);
            data.Rows[2].Value.Should().Be(62);
            data.SamplingFactor.Should().Be(1);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetOperatingSystems_Works_With_No_Data()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();

            // Act
            var data = dataStore.GetOperatingSystems(null, null, null, 1);

            // Assert
            data.TotalRows.Should().Be(0);
            data.PageCount.Should().Be(0);
            data.Rows.Should().HaveCount(0);
            data.SamplingFactor.Should().Be(1);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetOperatingSystems_Works_With_No_Filter()
        {
            // Arrange
            var dataStore = CreatePersistentDataStore();

            // Act
            var data = dataStore.GetOperatingSystems(null, null, null, 1);

            // Assert
            data.TotalRows.Should().Be(4);
            data.PageCount.Should().Be(1);
            data.Rows.Should().HaveCount(4);
            data.Rows[0].Label.Should().Be("Windows");
            data.Rows[0].Percentage.Should().Be(40);
            data.Rows[0].Value.Should().Be(724);
            data.Rows[1].Label.Should().Be("iOS");
            data.Rows[1].Percentage.Should().Be(30);
            data.Rows[1].Value.Should().Be(543);
            data.Rows[2].Label.Should().Be("Mac");
            data.Rows[2].Percentage.Should().Be(20);
            data.Rows[2].Value.Should().Be(362);
            data.Rows[3].Label.Should().Be("Android");
            data.Rows[3].Percentage.Should().Be(10);
            data.Rows[3].Value.Should().Be(181);
            data.SamplingFactor.Should().Be(1);
        }

        [Fact]
        public void Test_SqlServerDataStore_GetOperatingSystems_Works_With_Filters()
        {
            // Arrange
            var dataStore = CreatePersistentDataStore();

            // Act
            var data = dataStore.GetOperatingSystems(DateTime.Parse("2023-01-01"), DateTime.Parse("2023-01-31"), null, 1);

            // Assert
            data.TotalRows.Should().Be(4);
            data.PageCount.Should().Be(1);
            data.Rows.Should().HaveCount(4);
            data.Rows[0].Label.Should().Be("Windows");
            data.Rows[0].Percentage.Should().Be(40);
            data.Rows[0].Value.Should().Be(124);
            data.Rows[1].Label.Should().Be("iOS");
            data.Rows[1].Percentage.Should().Be(30);
            data.Rows[1].Value.Should().Be(93);
            data.Rows[2].Label.Should().Be("Mac");
            data.Rows[2].Percentage.Should().Be(20);
            data.Rows[2].Value.Should().Be(62);
            data.Rows[3].Label.Should().Be("Android");
            data.Rows[3].Percentage.Should().Be(10);
            data.Rows[3].Value.Should().Be(31);
            data.SamplingFactor.Should().Be(1);
        }
    }
}
