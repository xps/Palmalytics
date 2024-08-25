using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using BitFaster.Caching.Lru;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Palmalytics.Extensions;
using Palmalytics.Model;
using Palmalytics.SqlServer.Extensions;
using Palmalytics.SqlServer.Scripts;
using Palmalytics.SqlServer.Utilities;
using Palmalytics.Utilities;

namespace Palmalytics.SqlServer
{
    public class SqlServerDataStore : IDataStore
    {
        private readonly SqlServerOptions options;
        private readonly ILogger<SqlServerDataStore> logger;

        private readonly ConcurrentLru<string, int> rowCountCache = new(100);
        private static readonly ConcurrentLru<IPAddress, string> geocodingCache = new(500); // TODO: config option for cache size

        // TODO: config options
        private const int pageSize = 10;
        private const int maxRows = 1_000_000;
        private const int cacheDurationMinutes = 5;

        // Shortcuts to access schema and table names from the options
        private string schema => options.Schema;
        private string requestsTable => options.RequestsTable;
        private string sessionsTable => options.SessionsTable;
        private string geocodingTable => options.GeocodingTable;
        private string settingsTable => options.SettingsTable;

        // Allows unit tests to access the options
        internal SqlServerOptions Options => options;

        public SqlServerDataStore(
            IOptions<object> options,
            ILogger<SqlServerDataStore> logger)
        {
            this.logger = logger;

            if (options.Value is SqlServerOptions sqlServerOptions)
                this.options = sqlServerOptions;
            else
                throw new InvalidOperationException("Options type is not SqlServerOptions");

            if (string.IsNullOrWhiteSpace(this.options.ConnectionString))
                throw new InvalidOperationException("No connection string in options");
            if (string.IsNullOrWhiteSpace(this.options.Schema))
                throw new InvalidOperationException("No schema in options");
        }

        public void Initialize()
        {
            CreateOrUpdateSchema();
        }

        #region Ingestion

        public async Task AddRequestAsync(RequestData requestData)
        {
            using var ingestionWritter = new IngestionWritter(options);
            await ingestionWritter.AddRequestAsync(requestData);
        }

        //private void HandleSqlTimeout(SqlConnection connection, TimeSpan elapsed)
        //{
        //    logger.LogWarning("SQL Server timeout (elapsed: {elapsed})", elapsed);
        //    try
        //    {
        //        var csv = connection.ExecuteReaderCsv("EXEC dbo.GetCurrentQueries", x => x.CommandTimeout = 1);
        //        logger.LogDebug("GetCurrentQueries returned:\n{csv}", csv);
        //    }
        //    catch (Exception x)
        //    {
        //        logger.LogWarning(x, "Could not execute GetCurrentQueries");
        //    }
        //}

        #endregion

        #region Data Queries

        public int GetRequestCount()
        {
            return QuerySingle<int>($"SELECT COUNT(*) FROM [{schema}].[{requestsTable}]");
        }

        public int GetSessionCount()
        {
            return QuerySingle<int>($"SELECT COUNT(*) FROM [{schema}].[{sessionsTable}]");
        }

        public List<Request> GetLastRequests(int count = 100)
        {
            var sql = $"""
                SELECT TOP {count} * FROM [{schema}].[{requestsTable}] ORDER BY Id DESC
            """;

            return Query<Request>(sql);
        }

        public List<Session> GetLastSessions(int count = 100)
        {
            var sql = $"""
                SELECT TOP {count} * FROM [{schema}].[{sessionsTable}] ORDER BY Id DESC
            """;

            return Query<Session>(sql);
        }

        public TopData GetTopData(DateTime? dateFrom, DateTime? dateTo, Filters filters)
        {
            var estimatedRowCount = EstimateSessionCount(dateFrom, dateTo);
            var samplingFactor = GetSamplingFactor(estimatedRowCount);

            var query = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"{samplingFactor} * COUNT(1) AS [TotalSessions]")
                .Select($"{samplingFactor} * ISNULL(SUM([RequestCount]), 0) AS [TotalPageViews]")
                .Select($"ROUND(ISNULL(100.0 * SUM(CAST([IsBounce] AS INT)) / (CASE WHEN COUNT(1) > 0 THEN COUNT(1) ELSE 1 END), 0), 0) AS [AverageBounceRate]")
                .Select($"ISNULL(AVG([Duration]), 0) AS [AverageSessionDuration]")
                .Select($"1.0 * ISNULL(SUM([RequestCount]), 0) / (CASE WHEN COUNT(1) > 0 THEN COUNT(1) ELSE 1 END) AS [AveragePagesPerSession]")
                .Select($"{samplingFactor} AS [SamplingFactor]")
                .WhereDates(dateFrom, dateTo)
                .WhereFilters(filters)
                .WhereSampling(samplingFactor)
                .GetTemplate();

            return QuerySingle<TopData>(query);
        }

        // TODO: constants for properties
        public ChartData GetChart(DateTime? dateFrom, DateTime? dateTo, Interval interval, string property, Filters filters)
        {
            var estimatedRowCount = EstimateSessionCount(dateFrom, dateTo);
            var samplingFactor = GetSamplingFactor(estimatedRowCount);

            var multiplier =
                property is ChartProperty.Sessions or ChartProperty.PageViews ? samplingFactor :
                1;

            var select = property switch
            {
                ChartProperty.Sessions => "COUNT(1)",
                ChartProperty.PageViews => "SUM([RequestCount])",
                ChartProperty.BounceRate => "CAST(ROUND(100.0 * SUM(CAST([IsBounce] AS INT)) / COUNT(1), 0) AS INT)",
                ChartProperty.SessionDuration => "AVG([Duration])",
                ChartProperty.PagesPerSession => "CAST(SUM([RequestCount]) AS FLOAT) / COUNT(1)",
                _ => throw new ArgumentException($"Invalid property: {property}", nameof(property))
            };

            // THINK: can we do better than use the start date? that could be inaccurate.
            var groupBy = interval switch
            {
                Interval.Days => $"CAST([DateStartedUtc] AS Date)",
                Interval.Weeks => $"DATEADD(WEEK, DATEDIFF(WEEK, 0, [DateStartedUtc]), 0)",
                Interval.Months => $"DATEADD(MONTH, DATEDIFF(MONTH, 0, [DateStartedUtc]), 0)",
                Interval.Years => $"DATEADD(YEAR, DATEDIFF(YEAR, 0, [DateStartedUtc]), 0)",
                _ => throw new ArgumentException($"Invalid interval: {interval}", nameof(interval))
            };

            var query = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"{groupBy} as [Date]")
                .Select($"{multiplier} * {select} as [Value]")
                .WhereDates(dateFrom, dateTo)
                .WhereFilters(filters)
                .WhereSampling(samplingFactor)
                .GroupBy(groupBy)
                .GetTemplate();

            var chartData = Query<ChartDataItem>(query);

            // Fill in missing dates with zeros
            var from = dateFrom;
            var to = dateTo;
            if (chartData.Any())
            {
                var dates = chartData.Select(x => x.Date).ToHashSet();

                from ??= chartData.Min(x => x.Date);
                to ??= chartData.Max(x => x.Date);

                var reportingPeriod = DateHelpers.GetReportingPeriod(from.Value, to.Value, interval);
                if (interval == Interval.Days)
                {
                    for (var date = reportingPeriod.Start; date <= reportingPeriod.End; date = date.AddDays(1))
                        if (!dates.Contains(date))
                            chartData.Add(new ChartDataItem { Date = date, Value = 0 });
                }
                if (interval == Interval.Weeks)
                {
                    for (var date = reportingPeriod.Start; date <= reportingPeriod.End; date = date.AddDays(7))
                        if (!dates.Contains(date))
                            chartData.Add(new ChartDataItem { Date = date, Value = 0 });
                }
                if (interval == Interval.Months)
                {
                    for (var date = reportingPeriod.Start; date <= reportingPeriod.End; date = date.AddMonths(1))
                        if (!dates.Contains(date))
                            chartData.Add(new ChartDataItem { Date = date, Value = 0 });
                }
                if (interval == Interval.Years)
                {
                    for (var date = reportingPeriod.Start; date <= reportingPeriod.End; date = date.AddYears(1))
                        if (!dates.Contains(date))
                            chartData.Add(new ChartDataItem { Date = date, Value = 0 });
                }
            }

            return new ChartData
            {
                TotalDays = to != null && from != null ? (int)(to - from)?.TotalDays + 1 : null,
                DateFrom = from,
                DateTo = to,
                Data = chartData.OrderBy(x => x.Date).ToList(),
                SamplingFactor = multiplier
            };
        }

        public TableData GetBrowsers(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            var estimatedRowCount = EstimateSessionCount(dateFrom, dateTo);
            var samplingFactor = GetSamplingFactor(estimatedRowCount);

            var column = string.IsNullOrWhiteSpace(filters?.Browser) ?
                "BrowserName" :
                "BrowserVersion";

            var where = (SessionQueryBuilder sqlBuilder) => sqlBuilder
                .WhereDates(dateFrom, dateTo)
                .WhereFilters(filters)
                .Where($"{column} IS NOT NULL")
                .WhereSampling(samplingFactor);

            var totalQuery = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"COUNT(DISTINCT {column}) AS [RowCount]")
                .Select($"{samplingFactor} * COUNT(1) AS [Total]")
                .Where(where)
                .GetTemplate();

            var (totalRows, totalSessions) = QuerySingle<(int, int)>(totalQuery.RawSql, totalQuery.Parameters);

            var query = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"{column} AS [Label]")
                .Select($"{samplingFactor} * COUNT(1) AS [Value]")
                .Select($"100.0 * {samplingFactor} * COUNT(1) / {totalSessions} AS [Percentage]")
                .Where(where)
                .GroupBy(column)
                .OrderBy("COUNT(1) DESC")
                .Paging(page, pageSize)
                .GetTemplate();

            var data = Query<TableDataItem>(query.RawSql, query.Parameters).ToList();

            return new TableData
            {
                TotalRows = totalRows,
                PageCount = (int)Math.Ceiling(totalRows / (float)pageSize),
                Rows = data,
                SamplingFactor = samplingFactor
            };
        }

        public TableData GetOperatingSystems(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            var estimatedRowCount = EstimateSessionCount(dateFrom, dateTo);
            var samplingFactor = GetSamplingFactor(estimatedRowCount);

            var column = string.IsNullOrWhiteSpace(filters?.OS) ?
                "OSName" :
                "OSVersion";

            var where = (SessionQueryBuilder sqlBuilder) => sqlBuilder
                .WhereDates(dateFrom, dateTo)
                .WhereFilters(filters)
                .Where($"{column} IS NOT NULL")
                .WhereSampling(samplingFactor);

            var totalQuery = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"COUNT(DISTINCT {column}) AS [RowCount]")
                .Select($"{samplingFactor} * COUNT(1) AS [Total]")
                .Where(where)
                .GetTemplate();

            var (totalRows, totalSessions) = QuerySingle<(int, int)>(totalQuery.RawSql, totalQuery.Parameters);

            var query = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"{column} AS [Label]")
                .Select($"{samplingFactor} * COUNT(1) AS [Value]")
                .Select($"100.0 * {samplingFactor} * COUNT(1) / {totalSessions} AS [Percentage]")
                .Where(where)
                .GroupBy(column)
                .OrderBy("COUNT(1) DESC")
                .Paging(page, pageSize)
                .GetTemplate();

            var data = Query<TableDataItem>(query.RawSql, query.Parameters).ToList();

            return new TableData
            {
                TotalRows = totalRows,
                PageCount = (int)Math.Ceiling(totalRows / (float)pageSize),
                Rows = data,
                SamplingFactor = samplingFactor
            };
        }

        public TableData GetReferrers(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            var estimatedRowCount = EstimateSessionCount(dateFrom, dateTo);
            var samplingFactor = GetSamplingFactor(estimatedRowCount);

            var column = string.IsNullOrWhiteSpace(filters?.Referrer) ?
                "ReferrerName" :
                "Referrer";

            var where = (SessionQueryBuilder sqlBuilder) => sqlBuilder
                .WhereDates(dateFrom, dateTo)
                .WhereFilters(filters)
                .Where($"{column} IS NOT NULL")
                .WhereSampling(samplingFactor);

            var totalQuery = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"COUNT(DISTINCT {column}) AS [RowCount]")
                .Select($"{samplingFactor} * COUNT(1) AS [Total]")
                .Where(where)
                .GetTemplate();

            var (totalRows, totalSessions) = QuerySingle<(int, int)>(totalQuery.RawSql, totalQuery.Parameters);

            var query = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"{column} AS [Label]")
                .Select($"{samplingFactor} * COUNT(1) AS [Value]")
                .Select($"100.0 * {samplingFactor} * COUNT(1) / {totalSessions} AS [Percentage]")
                .Where(where)
                .GroupBy(column)
                .OrderBy("COUNT(1) DESC")
                .Paging(page, pageSize)
                .GetTemplate();

            var data = Query<TableDataItem>(query.RawSql, query.Parameters).ToList();

            return new TableData
            {
                TotalRows = totalRows,
                PageCount = (int)Math.Ceiling(totalRows / (float)pageSize),
                Rows = data,
                SamplingFactor = samplingFactor
            };
        }

        public TableData GetCountries(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            var estimatedRowCount = EstimateSessionCount(dateFrom, dateTo);
            var samplingFactor = GetSamplingFactor(estimatedRowCount);

            var where = (SessionQueryBuilder sqlBuilder) => sqlBuilder
                .WhereDates(dateFrom, dateTo)
                .WhereFilters(filters)
                .Where($"[Country] IS NOT NULL")
                .WhereSampling(samplingFactor);

            var totalQuery = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"COUNT(DISTINCT [Country]) AS [RowCount]")
                .Select($"{samplingFactor} * COUNT(1) AS [Total]")
                .Where(where)
                .GetTemplate();

            var (totalRows, totalSessions) = QuerySingle<(int, int)>(totalQuery.RawSql, totalQuery.Parameters);

            var query = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"[Country] AS [Label]")
                .Select($"{samplingFactor} * COUNT(1) AS [Value]")
                .Select($"100.0 * {samplingFactor} * COUNT(1) / {totalSessions} AS [Percentage]")
                .Where(where)
                .GroupBy("[Country]")
                .OrderBy("COUNT(1) DESC")
                .Paging(page, pageSize)
                .GetTemplate();

            var data = Query<TableDataItem>(query.RawSql, query.Parameters).ToList();

            return new TableData
            {
                TotalRows = totalRows,
                PageCount = (int)Math.Ceiling(totalRows / (float)pageSize),
                Rows = data,
                SamplingFactor = samplingFactor
            };
        }

        public TableData GetTopPages(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            var estimatedRowCount = EstimateRequestCount(dateFrom, dateTo);
            var samplingFactor = GetSamplingFactor(estimatedRowCount);

            var where = (RequestQueryBuilder sqlBuilder) => sqlBuilder
                .WhereDates(dateFrom, dateTo)
                .WhereFilters(filters)
                .WhereSampling(samplingFactor);

            var totalQuery = new RequestQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"COUNT(DISTINCT [Path]) AS [RowCount]")
                .Select($"{samplingFactor} * COUNT(1) AS [Total]")
                .Where(where)
                .GetTemplate();

            var (totalRows, totalSessions) = QuerySingle<(int, int)>(totalQuery.RawSql, totalQuery.Parameters);

            var query = new RequestQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"[Path] AS [Label]")
                .Select($"{samplingFactor} * COUNT(1) AS [Value]")
                .Select($"100.0 * {samplingFactor} * COUNT(1) / {totalSessions} AS [Percentage]")
                .Where(where)
                .GroupBy("[Path]")
                .OrderBy("COUNT(1) DESC")
                .Paging(page, pageSize)
                .GetTemplate();

            var data = Query<TableDataItem>(query.RawSql, query.Parameters).ToList();

            return new TableData
            {
                TotalRows = totalRows,
                PageCount = (int)Math.Ceiling(totalRows / (float)pageSize),
                Rows = data,
                SamplingFactor = samplingFactor
            };
        }

        public TableData GetEntryPages(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            var estimatedRowCount = EstimateSessionCount(dateFrom, dateTo);
            var samplingFactor = GetSamplingFactor(estimatedRowCount);

            var where = (SessionQueryBuilder sqlBuilder) => sqlBuilder
                .WhereDates(dateFrom, dateTo)
                .WhereFilters(filters)
                .WhereSampling(samplingFactor);

            var totalQuery = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"COUNT(DISTINCT [EntryPath]) AS [RowCount]")
                .Select($"{samplingFactor} * COUNT(1) AS [Total]")
                .Where(where)
                .GetTemplate();

            var (totalRows, totalSessions) = QuerySingle<(int, int)>(totalQuery.RawSql, totalQuery.Parameters);

            var query = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"[EntryPath] AS [Label]")
                .Select($"{samplingFactor} * COUNT(1) AS [Value]")
                .Select($"100.0 * {samplingFactor} * COUNT(1) / {totalSessions} AS [Percentage]")
                .Where(where)
                .GroupBy("[EntryPath]")
                .OrderBy("COUNT(1) DESC")
                .Paging(page, pageSize)
                .GetTemplate();

            var data = Query<TableDataItem>(query.RawSql, query.Parameters).ToList();

            return new TableData
            {
                TotalRows = totalRows,
                PageCount = (int)Math.Ceiling(totalRows / (float)pageSize),
                Rows = data,
                SamplingFactor = samplingFactor
            };
        }

        public TableData GetExitPages(DateTime? dateFrom, DateTime? dateTo, Filters filters, int page)
        {
            var estimatedRowCount = EstimateSessionCount(dateFrom, dateTo);
            var samplingFactor = GetSamplingFactor(estimatedRowCount);

            var where = (SessionQueryBuilder sqlBuilder) => sqlBuilder
                .WhereDates(dateFrom, dateTo)
                .WhereFilters(filters)
                .WhereSampling(samplingFactor);

            var totalQuery = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"COUNT(DISTINCT [ExitPath]) AS [RowCount]")
                .Select($"{samplingFactor} * COUNT(1) AS [Total]")
                .Where(where)
                .GetTemplate();

            var (totalRows, totalSessions) = QuerySingle<(int, int)>(totalQuery.RawSql, totalQuery.Parameters);

            var query = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                .Select($"[ExitPath] AS [Label]")
                .Select($"{samplingFactor} * COUNT(1) AS [Value]")
                .Select($"100.0 * {samplingFactor} * COUNT(1) / {totalSessions} AS [Percentage]")
                .Where(where)
                .GroupBy("[ExitPath]")
                .OrderBy("COUNT(1) DESC")
                .Paging(page, pageSize)
                .GetTemplate();

            var data = Query<TableDataItem>(query.RawSql, query.Parameters).ToList();

            return new TableData
            {
                TotalRows = totalRows,
                PageCount = (int)Math.Ceiling(totalRows / (float)pageSize),
                Rows = data,
                SamplingFactor = samplingFactor
            };
        }

        private int EstimateRequestCount(DateTime? dateFrom, DateTime? dateTo)
        {
            return rowCountCache.GetOrAdd(
                $"EstimateRequestCount_{dateFrom:yyyyMMdd}_{dateTo:yyyyMMdd}",
                key =>
                {
                    var query = new RequestQueryBuilder(schema, requestsTable, sessionsTable)
                        .Select($"1000 * COUNT(1)")
                        .WhereDates(dateFrom, dateTo)
                        .WhereSampling(1000)
                        .GetTemplate();

                    return QuerySingle<int>(query);
                }
            );
        }

        private int EstimateSessionCount(DateTime? dateFrom, DateTime? dateTo)
        {
            return rowCountCache.GetOrAdd(
                $"EstimateSessionCount_{dateFrom:yyyyMMdd}_{dateTo:yyyyMMdd}",
                key =>
                {
                    var query = new SessionQueryBuilder(schema, requestsTable, sessionsTable)
                        .Select($"1000 * COUNT(1)")
                        .WhereDates(dateFrom, dateTo)
                        .WhereSampling(1000)
                        .GetTemplate();

                    return QuerySingle<int>(query);
                }
            );
        }

        private int GetSamplingFactor(int rowCount)
        {
            if (rowCount > maxRows)
                return 1000;
            if (rowCount > maxRows / 10)
                return 100;
            if (rowCount > maxRows / 100)
                return 10;

            return 1;
        }

        #endregion

        #region Settings

        public void SaveSettings(Settings settings)
        {
            var script = new StringBuilder();

            script.AppendLine("BEGIN TRANSACTION");
            script.AppendLine("SET XACT_ABORT ON");

            script.AppendLine($"TRUNCATE TABLE [{schema}].[{settingsTable}]");

            var i = 0;
            var parameters = new Dictionary<string, object>();

            foreach (var property in settings.GetType().GetProperties())
            {
                var value = property.GetValue(settings)?.ToString();

                script.AppendLine($"""
                    INSERT INTO [{schema}].[{settingsTable}] (Name, Value) VALUES (@Name{i}, @Value{i});
                """);

                parameters.Add($"Name{i}", property.Name);
                parameters.Add($"Value{i}", value);

                i++;
            }

            script.AppendLine("COMMIT");

            Execute(script.ToString(), parameters);
        }

        public void SaveSetting<T>(string name, T value)
        {
            var sql = $"""
                BEGIN TRANSACTION
                SET XACT_ABORT ON

                DELETE FROM [{schema}].[{settingsTable}] WHERE [Name] = @Name;
                INSERT INTO [{schema}].[{settingsTable}] ([Name], [Value]) VALUES (@Name, @Value);

                COMMIT
            """;

            Execute(sql, new { name, value = value.ToString() });
        }

        public Settings GetSettings()
        {
            var sql = $"""
                SELECT * FROM [{schema}].[{settingsTable}]
            """;

            var rows = Query<(string Name, string Value)>(sql);

            var settings = new Settings();
            foreach (var row in rows)
            {
                var property = settings.GetType().GetProperty(row.Name);
                if (property != null)
                {
                    var value = !string.IsNullOrWhiteSpace(row.Value) ?
                        Convert.ChangeType(row.Value, property.PropertyType) :
                        null;

                    property.SetValue(settings, value);
                }
            }

            return settings;
        }

        public T GetSetting<T>(string name)
        {
            var sql = $"""
                SELECT [Value] FROM [{schema}].[{settingsTable}] WHERE [Name] = @Name
            """;

            var rows = QuerySingle<string>(sql, new { name });

            return !string.IsNullOrWhiteSpace(rows) ?
                (T)Convert.ChangeType(rows, typeof(T)) :
                default;
        }

        #endregion

        #region Schema

        public int GetSchemaVersion()
        {
            if (TableExists(settingsTable))
            {
                var sql = $"""
                    SELECT Value FROM [{schema}].[{settingsTable}] WHERE Name = 'SchemaVersion'
                """;

                var value = QuerySingle<string>(sql);
                if (!string.IsNullOrWhiteSpace(value))
                    return Convert.ToInt32(value);
            }

            return 0;
        }

        private void RunScript(string filename)
        {
            var sql = ResourceHelpers.GetScriptContent(filename);

            // TODO: test
            sql = Regex.Replace(sql, @"\{\w+\}", match =>
            {
                return match.Value switch
                {
                    "{schema}" => schema,
                    "{requestsTable}" => requestsTable,
                    "{sessionsTable}" => sessionsTable,
                    "{geocodingTable}" => geocodingTable,
                    "{settingsTable}" => settingsTable,
                    _ => throw new Exception($"Unknown script placeholder: {match.Value}"),
                };
            });

            Execute(sql, commandTimeout: 60 * 5);
        }

        private void CreateSchema()
        {
            Execute($"CREATE SCHEMA [{schema}]");
        }

        private bool SchemaExists()
        {
            var count = QuerySingle<int>(
                $"SELECT COUNT(*) FROM [sys].[schemas] WHERE [name] = @Schema",
                new { schema }
            );

            return count > 0;
        }

        private bool TableExists(string tableName)
        {
            var count = QuerySingle<int>(
                $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @TableName",
                new { schema, tableName }
            );

            return count > 0;
        }

        public void CreateOrUpdateSchema()
        {
            var connection = new SqlConnection(options.ConnectionString);

            // The connection has to stay open until we release the lock
            connection.Open();

            try
            {
                using (var @lock = connection.GetAppLock("Palmalytics_CreateOrUpdateSchema"))
                {

                    if (options.AlwaysDropAndCreateDatabase && SchemaExists())
                    {
                        logger.LogWarning("AlwaysDropAndCreateDatabase = true, dropping database!");
                        if (SchemaExists())
                            DropSchema();
                    }

                    if (!SchemaExists())
                    {
                        logger.LogDebug("Schema {schema} not found in database, creating...", schema);
                        CreateSchema();
                    }

                    var currentVersion = GetSchemaVersion();
                    logger.LogDebug("Current schema version: {version}", currentVersion);

                    var targetVersion = 1;
                    while (ResourceHelpers.ScriptExists($"CreateSchema_v{targetVersion}.sql"))
                    {
                        if (currentVersion < targetVersion)
                        {
                            logger.LogDebug("Applying migration script: CreateSchema_v{version}.sql", targetVersion);
                            using var transactionScope = new TransactionScope();
                            RunScript($"CreateSchema_v{targetVersion}.sql");
                            transactionScope.Complete();
                        }
                        else
                        {
                            logger.LogDebug("No need to apply migration script: CreateSchema_v{version}.sql", targetVersion);
                        }

                        targetVersion++;
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }

        public void DropSchema()
        {
            RunScript("DropSchema.sql");
        }

        #endregion

        #region Geocoding

        public bool NeedsGeocodingDatabase()
        {
            return QuerySingle<int>($"SELECT CASE WHEN EXISTS(SELECT 1 FROM [{schema}].[{geocodingTable}]) THEN 0 ELSE 1 END") == 1;
        }

        public string GetCountryCodeForIPAddress(IPAddress address)
        {
            return geocodingCache.GetOrAdd(address, key =>
            {
                var bytes = key.GetAddressBytes();
                var ipVersion = key.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 4 : 6;

                var sql = $"""
                    SELECT TOP 1 [Country]
                    FROM [{schema}].[{geocodingTable}]
                    WHERE [IPVersion] = @ipVersion AND @bytes BETWEEN [Start] AND [End]
                    ORDER BY [Start] DESC
                """;

                return Query<string>(sql, new { ipVersion, bytes }).SingleOrDefault();
            });
        }

        //private void AddIPRange(IPAddress start, IPAddress end, string country)
        //{
        //    Execute($"""
        //        INSERT INTO [{schema}].[{geocodingTable}]
        //            ([Start], [End], [Country])
        //        VALUES
        //            (@start, @end, @country)
        //    """,
        //    new { start = start.GetAddressBytes(), end = end.GetAddressBytes(), country });
        //    );
        //}

        // TODO: locking
        public void ImportGeocodingData(IEnumerable<GeolocRange> data)
        {
            logger.LogDebug("Importing {count:N0} geocoding data entries to database", data.Count());
            var stopwatch = Stopwatch.StartNew();

            // TODO: more performant version?
            // Maybe "0x" + BitConverter.ToString(address.GetAddressBytes()).Replace("-", "")
            static string hex(IPAddress address) =>
                address.GetAddressBytes().Aggregate("0x", (x, y) => x + y.ToString("X2"));

            Execute($"TRUNCATE TABLE [{schema}].[{geocodingTable}]");

            // TODO: use SqlBulkCopy
            foreach (var group in data.GroupBy(x => x.Start.AddressFamily))
            {
                var ipVersion = group.Key == System.Net.Sockets.AddressFamily.InterNetwork ? 4 : 6;
                foreach (var row in group.Chunk(1_000))
                {
                    var sql = new StringBuilder();
                    sql.Append($"INSERT INTO [{schema}].[{geocodingTable}] ([Start], [End], [IPVersion], [Country]) VALUES ");
                    sql.Append(row.Select(x => $"({hex(x.Start)}, {hex(x.End)}, {ipVersion}, '{x.Country}')").Join(","));
                    Execute(sql.ToString());
                }
            }

            logger.LogDebug("Imported geocoding data - {count:N0} entries in {milliseconds:N0} ms", data.Count(), stopwatch.ElapsedMilliseconds);
        }

        #endregion

        #region Dapper Helpers

        private List<T> Query<T>(SqlBuilder.Template query)
        {
            return Query<T>(query.RawSql, query.Parameters);
        }

        private List<T> Query<T>(string sql, object parameters = null)
        {
            using var connection = new SqlConnection(options.ConnectionString);
            try
            {
                return connection.Query<T>(sql, parameters).ToList();
            }
            catch (SqlException ex)
            {
                ex.Data["Sql.Statement"] = sql;
                ex.Data["Sql.Parameters"] = parameters;
                throw;
            }
        }

        private T QuerySingle<T>(SqlBuilder.Template query)
        {
            return QuerySingle<T>(query.RawSql, query.Parameters);
        }

        private T QuerySingle<T>(string sql, object parameters = null)
        {
            using var connection = new SqlConnection(options.ConnectionString);
            try
            {
                return connection.QuerySingle<T>(sql, parameters);
            }
            catch (SqlException ex)
            {
                ex.Data["Sql.Statement"] = sql;
                ex.Data["Sql.Parameters"] = parameters;
                throw;
            }
        }

        private int Execute(SqlBuilder.Template query, int? commandTimeout = null)
        {
            return Execute(query.RawSql, query.Parameters, commandTimeout: commandTimeout);
        }

        private int Execute(string sql, object parameters = null, int? commandTimeout = null)
        {
            using var connection = new SqlConnection(options.ConnectionString);
            try
            {
                return connection.Execute(sql, parameters, commandTimeout: commandTimeout);
            }
            catch (SqlException ex)
            {
                ex.Data["Sql.Statement"] = sql;
                ex.Data["Sql.Parameters"] = parameters;
                throw;
            }
        }

        #endregion
    }
}
