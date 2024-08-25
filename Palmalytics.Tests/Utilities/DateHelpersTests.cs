using Palmalytics.Model;
using Palmalytics.Utilities;

namespace Palmalytics.Tests.Utilities
{
    public class DateHelpersTests
    {
        [Theory]
        [InlineData("20230917-20231023", "2023-09-17", "2023-09-17", "2023-10-23")]
        [InlineData("today", "2023-09-17", "2023-09-17", "2023-09-17")]
        [InlineData("last-7-days", "2023-09-17", "2023-09-11", "2023-09-17")]
        [InlineData("last-30-days", "2023-09-17", "2023-08-19", "2023-09-17")]
        [InlineData("last-12-months", "2023-09-17", "2022-09-18", "2023-09-17")]
        [InlineData("month-to-date", "2023-09-01", "2023-09-01", "2023-09-01")]
        [InlineData("month-to-date", "2023-09-17", "2023-09-01", "2023-09-17")]
        [InlineData("last-month", "2023-09-01", "2023-08-01", "2023-08-31")]
        [InlineData("last-month", "2023-09-17", "2023-08-01", "2023-08-31")]
        [InlineData("last-month", "2023-09-30", "2023-08-01", "2023-08-31")]
        [InlineData("year-to-date", "2022-12-31", "2022-01-01", "2022-12-31")]
        [InlineData("year-to-date", "2023-01-01", "2023-01-01", "2023-01-01")]
        [InlineData("year-to-date", "2023-09-17", "2023-01-01", "2023-09-17")]
        [InlineData("last-year", "2023-01-01", "2022-01-01", "2022-12-31")]
        [InlineData("last-year", "2023-09-17", "2022-01-01", "2022-12-31")]
        [InlineData("last-year", "2023-12-31", "2022-01-01", "2022-12-31")]
        [InlineData("all-time", "2023-09-17", null, null)]
        public void GetDateRangeForPeriod_ReturnsCorrectDateRange(string period, string now, string expectedFrom, string expectedTo)
        {
            // Act
            var (dateFrom, dateTo) = DateHelpers.GetDateRangeForPeriod(period, Convert.ToDateTime(now));

            // Assert

            if (expectedFrom == null)
                dateFrom.Should().BeNull();
            else
                dateFrom.Should().Be(Convert.ToDateTime(expectedFrom));

            if (expectedTo == null)
                dateTo.Should().BeNull();
            else
                dateTo.Should().Be(Convert.ToDateTime(expectedTo));
        }

        [Theory]
        [InlineData("2023-01-03", "2023-01-03", Interval.Days, "2023-01-03", "2023-01-03")]
        [InlineData("2023-01-03", "2023-01-10", Interval.Days, "2023-01-03", "2023-01-10")]
        [InlineData("2023-01-03", "2023-01-03", Interval.Weeks, "2023-01-02", "2023-01-02")]
        [InlineData("2023-01-03", "2023-01-10", Interval.Weeks, "2023-01-02", "2023-01-09")]
        [InlineData("2023-01-01", "2023-01-15", Interval.Weeks, "2022-12-26", "2023-01-09")]
        [InlineData("2023-01-03", "2023-01-03", Interval.Months, "2023-01-01", "2023-01-01")]
        [InlineData("2023-01-03", "2023-01-10", Interval.Months, "2023-01-01", "2023-01-01")]
        [InlineData("2023-01-03", "2023-02-10", Interval.Months, "2023-01-01", "2023-02-01")]
        [InlineData("2023-01-03", "2023-01-03", Interval.Years, "2023-01-01", "2023-01-01")]
        [InlineData("2023-01-03", "2023-11-10", Interval.Years, "2023-01-01", "2023-01-01")]
        [InlineData("2023-01-03", "2024-02-10", Interval.Years, "2023-01-01", "2024-01-01")]
        public void Test_DateHelpers_GetReportingPeriod(string from, string to, Interval interval, string expectedStart, string expectedEnd)
        {
            var (start, end) = DateHelpers.GetReportingPeriod(DateTime.Parse(from), DateTime.Parse(to), interval);

            start.Should().Be(DateTime.Parse(expectedStart));
            end.Should().Be(DateTime.Parse(expectedEnd));
        }
    }
}
