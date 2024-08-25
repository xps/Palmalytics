using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Microsoft.Data.SqlClient;
using Palmalytics.Extensions;
using Palmalytics.Model;
using Palmalytics.SqlServer.Extensions;

namespace Palmalytics.SqlServer
{
    public class IngestionWritter(SqlServerOptions options) : IDisposable
    {
        private SqlConnection _connection;

        // Shortcuts to access schema and table names from the options
        private string schema => options.Schema;
        private string requestsTable => options.RequestsTable;
        private string sessionsTable => options.SessionsTable;

        public async Task AddRequestAsync(RequestData requestData)
        {
            Session session;

            _connection = new SqlConnection(options.ConnectionString);

            // The connection has to stay open until we release the lock
            await _connection.OpenAsync();

            try
            {
                await using (var @lock = await _connection.GetAppLockAsync("Palmalytics_Session_" + requestData.GetSessionHashCode()))
                {
                    using (var transactionScope = new TransactionScope())
                    {
                        session = FindSession(requestData);

                        if (session == null)
                        {
                            session = new Session(requestData);
                            InsertSession(session);
                        }
                        else
                            UpdateSession(session, requestData);

                        requestData.SessionId = session.Id;
                        InsertRequest(requestData);

                        transactionScope.Complete();
                    }
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        private void InsertRequest(RequestData requestData)
        {
            var sql = $"""
                INSERT INTO [{schema}].[{requestsTable}]
                (
                    SessionId,
                    DateUtc,
                    Path,
                    QueryString,
                    Referrer,
                    UtmSource,
                    UtmMedium,
                    UtmCampaign,
                    UtmTerm,
                    UtmContent,
                    UserName,
                    CustomData,
                    ResponseCode,
                    ResponseTime,
                    ContentType
                )
                VALUES
                (
                    @SessionId,
                    @DateUtc,
                    @Path,
                    @QueryString,
                    @Referrer,
                    @UtmSource,
                    @UtmMedium,
                    @UtmCampaign,
                    @UtmTerm,
                    @UtmContent,
                    @UserName,
                    @CustomData,
                    @ResponseCode,
                    @ResponseTime,
                    @ContentType
                );
            """;

            var parameters = new
            {
                SessionId = requestData.SessionId,
                DateUtc = requestData.DateUtc,
                Path = requestData.Path?.Left(1_000),
                QueryString = requestData.QueryString?.Left(1_000),
                Referrer = requestData.Referrer?.Left(1_000),
                UtmSource = requestData.UtmSource?.Left(50),
                UtmMedium = requestData.UtmMedium?.Left(50),
                UtmCampaign = requestData.UtmCampaign?.Left(50),
                UtmTerm = requestData.UtmTerm?.Left(50),
                UtmContent = requestData.UtmContent?.Left(50),
                UserName = requestData.UserName?.Left(50),
                CustomData = requestData.CustomData,
                ResponseCode = requestData.ResponseCode,
                ResponseTime = requestData.ResponseTime,
                ContentType = requestData.ContentType?.Left(50)
            };

            _connection.Execute(sql, parameters, commandTimeout: options.IngestionCommandTimeout);
        }

        private void InsertSession(Session session)
        {
            var sql = $"""
                INSERT INTO [{schema}].[{sessionsTable}]
                (
                    HashCode,
                    DateStartedUtc,
                    DateEndedUtc,

                    IPAddress,
                    UserAgent,
                    Language,
                    Country,
                    BrowserName,
                    BrowserVersion,
                    OSName,
                    OSVersion,

                    EntryPath,
                    ExitPath,
                    IsBounce,

                    Referrer,
                    ReferrerName,
                    UtmSource,
                    UtmMedium,
                    UtmCampaign,
                    UtmTerm,
                    UtmContent,
                    UserName,
                    CustomData,

                    Duration,
                    RequestCount
                )
                VALUES
                (
                    @HashCode,
                    @DateStartedUtc,
                    @DateEndedUtc,

                    @IPAddress,
                    @UserAgent,
                    @Language,
                    @Country, 
                    @BrowserName, 
                    @BrowserVersion, 
                    @OSName, 
                    @OSVersion, 

                    @EntryPath,
                    @ExitPath,
                    @IsBounce,

                    @Referrer,
                    @ReferrerName,
                    @UtmSource,
                    @UtmMedium,
                    @UtmCampaign,
                    @UtmTerm,
                    @UtmContent,
                    @UserName,
                    @CustomData,

                    @Duration,
                    @RequestCount
                );
                SELECT SCOPE_IDENTITY();
            """;

            var parameters = new
            {
                HashCode = session.HashCode,
                DateStartedUtc = session.DateStartedUtc,
                DateEndedUtc = session.DateEndedUtc,

                IPAddress = session.IPAddress?.Left(45),
                UserAgent = session.UserAgent?.Left(1_000),
                Language = session.Language?.Left(5),
                Country = session.Country?.Left(2),
                BrowserName = session.BrowserName?.Shorten(50),
                BrowserVersion = session.BrowserVersion?.Left(50),
                OSName = session.OSName?.Shorten(50),
                OSVersion = session.OSVersion?.Left(50),

                EntryPath = session.EntryPath?.Shorten(1_000),
                ExitPath = session.ExitPath?.Shorten(1_000),
                IsBounce = session.IsBounce,

                Referrer = session.Referrer?.Left(1_000),
                ReferrerName = session.ReferrerName?.Shorten(50),
                UtmSource = session.UtmSource?.Left(50),
                UtmMedium = session.UtmMedium?.Left(50),
                UtmCampaign = session.UtmCampaign?.Left(50),
                UtmTerm = session.UtmTerm?.Left(50),
                UtmContent = session.UtmContent?.Left(50),
                UserName = session.UserName?.Left(50),
                CustomData = session.CustomData,

                Duration = session.Duration,
                RequestCount = session.RequestCount
            };

            session.Id = (long)_connection.QuerySingle<decimal>(sql, parameters, commandTimeout: options.IngestionCommandTimeout);
        }

        private Session FindSession(RequestData requestData)
        {
            var sessions = _connection.Query<Session>(
                $"""
                    SELECT
                        *
                    FROM
                        [{schema}].[{sessionsTable}]
                    WHERE
                        HashCode = @HashCode AND
                        DateEndedUtc > @Date
                    ORDER BY
                        DateEndedUtc DESC
                """,
                new
                {
                    HashCode = requestData.GetSessionHashCode(),
                    Date = requestData.DateUtc.AddMinutes(-options.MinutesBeforeNewSession)
                },
                commandTimeout: options.IngestionCommandTimeout
            );

            return sessions
                .Where(requestData.MatchesSession)
                .FirstOrDefault();
        }

        private void UpdateSession(Session session, RequestData request)
        {
            if (request.DateUtc < session.DateStartedUtc)
                session.DateStartedUtc = request.DateUtc;

            if (request.DateUtc > session.DateEndedUtc)
                session.DateEndedUtc = request.DateUtc;

            if (request.DateUtc == session.DateStartedUtc)
                session.EntryPath = request.Path;

            if (request.DateUtc == session.DateEndedUtc)
                session.ExitPath = request.Path;

            session.Duration = (int)Math.Round((session.DateEndedUtc - session.DateStartedUtc).TotalSeconds);
            session.IsBounce = false;
            session.RequestCount++;

            var sql = $"""
                UPDATE [{schema}].[{sessionsTable}]
                SET
                    DateStartedUtc = @DateStartedUtc,
                    DateEndedUtc = @DateEndedUtc,
                    Duration = @Duration,
                    IsBounce = @IsBounce,
                    EntryPath = @EntryPath,
                    ExitPath = @ExitPath,
                    RequestCount = @RequestCount
                WHERE
                    Id = @Id
            """;

            _connection.Execute(sql, session, commandTimeout: options.IngestionCommandTimeout);
        }

        public void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();
        }
    }
}
