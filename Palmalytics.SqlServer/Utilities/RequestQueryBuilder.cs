using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using Palmalytics.Model;

namespace Palmalytics.SqlServer.Utilities
{
    public class RequestQueryBuilder(string schema, string requestsTable, string sessionsTable) : SqlBuilder
    {
        private readonly List<Clause> _clauses = new();

        public RequestQueryBuilder Where(Func<RequestQueryBuilder, RequestQueryBuilder> func)
        {
            func(this);

            return this;
        }

        public RequestQueryBuilder WhereDates(DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom != null)
                Where("r.[DateUtc] >= @DateFrom", new { dateFrom });
            if (dateTo != null)
                Where("r.[DateUtc] <= DATEADD(DAY, 1, @DateTo)", new { dateTo });

            return this;
        }

        //public RequestQueryBuilder WhereSessionDates(DateTime? dateFrom, DateTime? dateTo)
        //{
        //    if (dateFrom != null)
        //        Where("s.[DateStartedUtc] >= @DateFrom", new { dateFrom });
        //    if (dateTo != null)
        //        Where("s.[DateEndedUtc] < DATEADD(DAY, 1, @DateTo)", new { dateTo });

        //    return this;
        //}

        public RequestQueryBuilder WhereFilters(Filters filters)
        {
            if (!string.IsNullOrWhiteSpace(filters?.Referrer))
                Where("s.[ReferrerName] = @Referrer", new { filters.Referrer });
                
            if (!string.IsNullOrWhiteSpace(filters?.ReferrerUrl))
                Where("s.[Referrer] = @ReferrerUrl", new { filters.ReferrerUrl });

            if (filters?.UtmSource == "(not set)")
                Where("s.[UtmSource] IS NULL");
            else if (!string.IsNullOrWhiteSpace(filters?.UtmSource))
                Where("s.[UtmSource] = @UtmSource", new { filters.UtmSource });

            if (filters?.UtmMedium == "(not set)")
                Where("s.[UtmMedium] IS NULL");
            else if (!string.IsNullOrWhiteSpace(filters?.UtmMedium))
                Where("s.[UtmMedium] = @UtmMedium", new { filters.UtmMedium });

            if (filters?.UtmCampaign == "(not set)")
                Where("s.[UtmCampaign] IS NULL");
            else if (!string.IsNullOrWhiteSpace(filters?.UtmCampaign))
                Where("s.[UtmCampaign] = @UtmCampaign", new { filters.UtmCampaign });

            if (filters?.UtmContent == "(not set)")
                Where("s.[UtmContent] IS NULL");
            else if (!string.IsNullOrWhiteSpace(filters?.UtmContent))
                Where("s.[UtmContent] = @UtmContent", new { filters.UtmContent });

            if (filters?.UtmTerm == "(not set)")
                Where("s.[UtmTerm] IS NULL");
            else if (!string.IsNullOrWhiteSpace(filters?.UtmTerm))
                Where("s.[UtmTerm] = @UtmTerm", new { filters.UtmTerm });

            if (!string.IsNullOrWhiteSpace(filters?.Browser))
                Where("s.[BrowserName] = @Browser", new { filters.Browser });

            if (!string.IsNullOrWhiteSpace(filters?.BrowserVersion))
                Where("s.[BrowserVersion] = @BrowserVersion", new { filters.BrowserVersion });

            if (!string.IsNullOrWhiteSpace(filters?.OS))
                Where("s.[OSName] = @OS", new { filters.OS });

            if (!string.IsNullOrWhiteSpace(filters?.OSVersion))
                Where("s.[OSVersion] = @OSVersion", new { filters.OSVersion });

            if (!string.IsNullOrWhiteSpace(filters?.Country))
                Where("s.[Country] = @Country", new { filters.Country });

            if (!string.IsNullOrWhiteSpace(filters?.Path))
                Where("r.[Path] = @Path", new { filters.Path });

            if (!string.IsNullOrWhiteSpace(filters?.EntryPath))
                Where("s.[EntryPath] = @EntryPath", new { filters.EntryPath });

            if (!string.IsNullOrWhiteSpace(filters?.ExitPath))
                Where("s.[ExitPath] = @ExitPath", new { filters.ExitPath });

            return this;
        }

        public RequestQueryBuilder WhereSampling(int factor)
        {
            if (factor is 10 or 100 or 1000)
                Where($"r.[Sampling{factor}] = 1");
            else if (factor != 1)
                throw new ArgumentException("Sampling factor must be 10, 100, or 1000");

            return this;
        }

        //public RequestQueryBuilder WhereSessionSampling(bool shouldSample)
        //{
        //    if (shouldSample)
        //        Where("s.[Sampling] = 1");

        //    return this;
        //}

        public RequestQueryBuilder Paging(int page, int pageSize)
        {
            AddClause(
                "paging",
                $"OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                new { offset = (page - 1) * pageSize, pageSize },
                "");

            return this;
        }

        public Template GetTemplate()
        {
            var includeSessionsTable = _clauses.Any(x => Regex.IsMatch(x.Sql, @"\bs\."));

            return AddTemplate($"""
                SELECT
                    /**select**/
                FROM
                    [{schema}].[{requestsTable}] r
                        {(includeSessionsTable ? $"INNER JOIN [{schema}].[{sessionsTable}] s ON r.[SessionId] = s.[Id]" : "")}
                /**where**/
                /**groupby**/
                /**orderby**/
                /**paging**/
             """);
        }

        public new RequestQueryBuilder Where(string sql, object parameters = null)
        {
            _clauses.Add(new Clause { Type = "where", Sql = sql, Parameters = parameters });
            return (RequestQueryBuilder)base.Where(sql, parameters);
        }

        public new RequestQueryBuilder Select(string sql, object parameters = null)
        {
            _clauses.Add(new Clause { Type = "select", Sql = sql, Parameters = parameters });
            return (RequestQueryBuilder)base.Select(sql, parameters);
        }

        public new RequestQueryBuilder GroupBy(string sql, object parameters = null)
        {
            _clauses.Add(new Clause { Type = "groupby", Sql = sql, Parameters = parameters });
            return (RequestQueryBuilder)base.GroupBy(sql, parameters);
        }

        public new RequestQueryBuilder OrderBy(string sql, object parameters = null)
        {
            _clauses.Add(new Clause { Type = "orderby", Sql = sql, Parameters = parameters });
            return (RequestQueryBuilder)base.OrderBy(sql, parameters);
        }

        private class Clause
        {
            public string Type { get; set; }
            public string Sql { get; set; }
            public object Parameters { get; set; }
        }
    }
}