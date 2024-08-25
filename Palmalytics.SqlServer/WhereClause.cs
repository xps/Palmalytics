using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Palmalytics.Extensions;
using Palmalytics.Model;

namespace Palmalytics.SqlServer
{
    // TODO: unit tests
    public class WhereClause : List<string>
    {
        private DateTime? dateFrom;
        private DateTime? dateTo;
        private Filters filters;

        private WhereClause()
        {
        }

        public static WhereClause CreateForSessions(DateTime? dateFrom, DateTime? dateTo, Filters filters)
        {
            var whereClause = new WhereClause
            {
                dateFrom = dateFrom,
                dateTo = dateTo,
                filters = filters
            };

            if (dateFrom != null)
                whereClause.Add("DateStartedUtc >= @DateFrom");
            if (dateTo != null)
                whereClause.Add("DateEndedUtc <= DATEADD(DAY, 1, @DateTo)");

            if (!string.IsNullOrWhiteSpace(filters?.Browser))
                whereClause.Add("BrowserName = @BrowserName");
            if (!string.IsNullOrWhiteSpace(filters?.BrowserVersion))
                whereClause.Add("BrowserVersion = @BrowserVersion");

            if (!string.IsNullOrWhiteSpace(filters?.OS))
                whereClause.Add("OSName = @OSName");
            if (!string.IsNullOrWhiteSpace(filters?.OSVersion))
                whereClause.Add("OSVersion = @OSVersion");

            return whereClause;
        }

        public static WhereClause CreateForRequests(DateTime? dateFrom, DateTime? dateTo, Filters filters)
        {
            var whereClause = new WhereClause();

            if (dateFrom != null)
                whereClause.Add("DateUtc >= @DateFrom");
            if (dateTo != null)
                whereClause.Add("DateUtc <= DATEADD(DAY, 1, @DateTo)");

            return whereClause;
        }

        public IEnumerable<SqlParameter> GetParameters()
        {
            if (dateFrom != null)
                yield return new SqlParameter("@DateFrom", dateFrom);
            if (dateTo != null)
                yield return new SqlParameter("@DateTo", dateTo);

            if (!string.IsNullOrWhiteSpace(filters?.Browser))
                yield return new SqlParameter("@BrowserName", filters.Browser);
            if (!string.IsNullOrWhiteSpace(filters?.BrowserVersion))
                yield return new SqlParameter("@BrowserVersion", filters.BrowserVersion);

            if (!string.IsNullOrWhiteSpace(filters?.OS))
                yield return new SqlParameter("@OSName", filters.OS);
            if (!string.IsNullOrWhiteSpace(filters?.OSVersion))
                yield return new SqlParameter("@OSVersion", filters.OSVersion);
        }

        override public string ToString()
        {
            if (this.Any())
                return "WHERE " + this.Join(" AND ");
            else
                return string.Empty;
        }
    }
}
