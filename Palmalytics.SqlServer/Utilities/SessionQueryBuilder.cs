using System;
using Dapper;
using Palmalytics.Model;

namespace Palmalytics.SqlServer.Utilities
{
    public class SessionQueryBuilder(string schema, string requestsTable, string sessionsTable) : SqlBuilder
    {
        public SessionQueryBuilder Where(Func<SessionQueryBuilder, SessionQueryBuilder> func)
        {
            func(this);

            return this;
        }

        public SessionQueryBuilder WhereDates(DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom != null)
                Where("[DateStartedUtc] >= @DateFrom", new { dateFrom });
            if (dateTo != null)
                Where("[DateEndedUtc] < DATEADD(DAY, 1, @DateTo)", new { dateTo });

            return this;
        }

        public SessionQueryBuilder WhereFilters(Filters filters)
        {
            if (!string.IsNullOrWhiteSpace(filters?.Referrer))
                Where("[ReferrerName] = @Referrer", new { filters.Referrer });

            if (!string.IsNullOrWhiteSpace(filters?.ReferrerUrl))
                Where("[Referrer] = @ReferrerUrl", new { filters.ReferrerUrl });

            if (filters?.UtmSource == "(not set)")
                Where("[UtmSource] IS NULL");
            else if (!string.IsNullOrWhiteSpace(filters?.UtmSource))
                Where("[UtmSource] = @UtmSource", new { filters.UtmSource });

            if (filters?.UtmMedium == "(not set)")
                Where("[UtmMedium] IS NULL");
            else if (!string.IsNullOrWhiteSpace(filters?.UtmMedium))
                Where("[UtmMedium] = @UtmMedium", new { filters.UtmMedium });

            if (filters?.UtmCampaign == "(not set)")
                Where("[UtmCampaign] IS NULL");
            else if (!string.IsNullOrWhiteSpace(filters?.UtmCampaign))
                Where("[UtmCampaign] = @UtmCampaign", new { filters.UtmCampaign });

            if (filters?.UtmContent == "(not set)")
                Where("[UtmContent] IS NULL");
            else if (!string.IsNullOrWhiteSpace(filters?.UtmContent))
                Where("[UtmContent] = @UtmContent", new { filters.UtmContent });

            if (filters?.UtmTerm == "(not set)")
                Where("[UtmTerm] IS NULL");
            else if (!string.IsNullOrWhiteSpace(filters?.UtmTerm))
                Where("[UtmTerm] = @UtmTerm", new { filters.UtmTerm });

            if (!string.IsNullOrWhiteSpace(filters?.Browser))
                Where("[BrowserName] = @Browser", new { filters.Browser });

            if (!string.IsNullOrWhiteSpace(filters?.BrowserVersion))
                Where("[BrowserVersion] = @BrowserVersion", new { filters.BrowserVersion });

            if (!string.IsNullOrWhiteSpace(filters?.OS))
                Where("[OSName] = @OS", new { filters.OS });

            if (!string.IsNullOrWhiteSpace(filters?.OSVersion))
                Where("[OSVersion] = @OSVersion", new { filters.OSVersion });

            if (!string.IsNullOrWhiteSpace(filters?.Country))
                Where("[Country] = @Country", new { filters.Country });

            if (!string.IsNullOrWhiteSpace(filters?.Path))
                Where($"[Id] IN (SELECT SessionId FROM [{schema}].[{requestsTable}] WHERE [Path] = @Path)", new { filters.Path });

            if (!string.IsNullOrWhiteSpace(filters?.EntryPath))
                Where("[EntryPath] = @EntryPath", new { filters.EntryPath });

            if (!string.IsNullOrWhiteSpace(filters?.ExitPath))
                Where("[ExitPath] = @ExitPath", new { filters.ExitPath });

            return this;
        }

        public SessionQueryBuilder WhereSampling(int factor)
        {
            if (factor is 10 or 100 or 1000)
                Where($"[Sampling{factor}] = 1");
            else if (factor != 1)
                throw new ArgumentException("Sampling factor must be 10, 100, or 1000");

            return this;
        }

        public SessionQueryBuilder Paging(int page, int pageSize)
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
            return AddTemplate($"""
                SELECT
                    /**select**/
                FROM
                    [{schema}].[{sessionsTable}]
                /**where**/
                /**groupby**/
                /**orderby**/
                /**paging**/
             """);
        }

        public new SessionQueryBuilder Where(string sql, object parameters = null) => (SessionQueryBuilder)base.Where(sql, parameters);
        public new SessionQueryBuilder Select(string sql, object parameters = null) => (SessionQueryBuilder)base.Select(sql, parameters);
        public new SessionQueryBuilder GroupBy(string sql, object parameters = null) => (SessionQueryBuilder)base.GroupBy(sql, parameters);
        public new SessionQueryBuilder OrderBy(string sql, object parameters = null) => (SessionQueryBuilder)base.OrderBy(sql, parameters);
    }
}