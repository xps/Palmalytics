using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Palmalytics.Model;

namespace Palmalytics.Utilities
{
    internal static class DateHelpers
    {
        /// <summary>
        /// This returns the dates matching a named period like 'last-30-days' or 'last-year'.
        /// </summary>
        public static (DateTime? DateFrom, DateTime? DateTo) GetDateRangeForPeriod(string period, DateTime? now = null)
        {
            if (string.IsNullOrWhiteSpace(period))
                period = "all-time";

            period = period.Trim().ToLower();

            // We allow the user to specify a custom date range in the format 'yyyymmdd-yyyymmdd'
            if (Regex.IsMatch(period, @"^\d{8}\-\d{8}$"))
            {
                var dates = period.Split('-');
                if (!DateTime.TryParseExact(dates[0], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                    throw new FormatException($"'{dates[0]}' is not a valid date");
                if (!DateTime.TryParseExact(dates[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                    throw new FormatException($"'{dates[1]}' is not a valid date");
                if (from > to)
                    throw new ArgumentException("'from' cannot be greater than 'to'");
                return (from, to);
            }

            DateTime? dateFrom, dateTo;

            var today = now?.Date ?? DateTime.UtcNow.Date;

            // TODO: remove magic strings
            switch (period)
            {
                case "today":
                    dateFrom = today;
                    dateTo = today;
                    break;
                case "last-7-days":
                    dateFrom = today.AddDays(-6);
                    dateTo = today;
                    break;
                case "last-30-days":
                    dateFrom = today.AddDays(-29);
                    dateTo = today;
                    break;
                case "last-12-months":
                    dateFrom = today.AddYears(-1).AddDays(1);
                    dateTo = today;
                    break;
                case "month-to-date":
                    dateFrom = new DateTime(today.Year, today.Month, 1);
                    dateTo = today;
                    break;
                case "last-month":
                    dateFrom = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    dateTo = dateFrom.Value.AddDays(DateTime.DaysInMonth(dateFrom.Value.Year, dateFrom.Value.Month) - 1);
                    break;
                case "year-to-date":
                    dateFrom = new DateTime(today.Year, 1, 1);
                    dateTo = today;
                    break;
                case "last-year":
                    dateFrom = new DateTime(today.Year - 1, 1, 1);
                    dateTo = dateFrom.Value.AddYears(1).AddDays(-1);
                    break;
                case "all-time":
                    dateFrom = null;
                    dateTo = null;
                    break;
                default:
                    throw new ArgumentException($"Unknown period '{period}'", nameof(period));
            }

            return (dateFrom, dateTo);
        }

        /// <summary>
        /// This method returns the start and end date of the reporting period.
        /// For example, if the user filter is set to 3/Jan/2023 to 10/Jan/2023 and the reporting unit is set to weeks,
        /// Then the reporting period is 2 weeks: 3-8/Jan/2023 and 9-15/Jan/2023.
        /// The method will return (3/Jan/2023, 9/Jan/2023).
        /// </summary>
        public static (DateTime Start, DateTime End) GetReportingPeriod(DateTime from, DateTime to, Interval interval)
        {
            if (interval == Interval.Days)
            {
                return (from, to);
            }

            if (interval == Interval.Weeks)
            {
                var start = from;
                while (start.DayOfWeek != DayOfWeek.Monday)
                    start = start.AddDays(-1);

                var end = to;
                while (end.DayOfWeek != DayOfWeek.Monday)
                    end = end.AddDays(-1);

                return (start, end);
            }

            if (interval == Interval.Months)
            {
                var start = new DateTime(from.Year, from.Month, 1);
                var end = new DateTime(to.Year, to.Month, 1);

                return (start, end);
            }

            if (interval == Interval.Years)
            {
                var start = new DateTime(from.Year, 1, 1);
                var end = new DateTime(to.Year, 1, 1);

                return (start, end);
            }

            throw new NotImplementedException("Not implemented for period: " + interval);
        }
    }
}
