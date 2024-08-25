using System.Net;
using System.Reflection;
using FluentAssertions.Extensions;
using Palmalytics.Model;
using static Palmalytics.SqlServer.Tests.DataSetHelpers;
using static Palmalytics.SqlServer.Tests.DataStoreHelpers;

namespace Palmalytics.SqlServer.Tests.DataStore
{
    public class SqlServerDataStoreTests : IDisposable
    {
        [Fact]
        public void Test_TestRequest_Has_Values_For_All_Fields()
        {
            var request = CreateTestRequest();

            // Check that all properties of the test request have non default values
            var properties = request.GetType().GetProperties();
            var excluded = new[] { nameof(Request.Id), nameof(Request.SessionId), nameof(request.IsBot) }; // TODO: remove IsBot?
            foreach (var property in properties.Where(x => !excluded.Contains(x.Name)))
            {
                var value = property.GetValue(request);
                if (value == null)
                {
                    throw new Exception($"Property {property.Name} is null");
                }
                else if (value is string stringValue)
                {
                    if (string.IsNullOrWhiteSpace(stringValue))
                        throw new Exception($"Property {property.Name} is empty");
                }
                else if (value is int intValue)
                {
                    if (intValue == 0)
                        throw new Exception($"Property {property.Name} is 0");
                }
                else if (value is long longValue)
                {
                    if (longValue == 0)
                        throw new Exception($"Property {property.Name} is 0");
                }
                else if (value is DateTime dateTimeValue)
                {
                    if (dateTimeValue == default)
                        throw new Exception($"Property {property.Name} is default DateTime");
                }
                else if (value is double doubleValue)
                {
                    if (doubleValue == 0)
                        throw new Exception($"Property {property.Name} is 0");
                }
                else
                {
                    throw new Exception($"Property {property.Name} has an unknown type");
                }
            }
        }

        [Fact]
        public void Test_SqlServerDataStore_Gets_Config()
        {
            var dataStore = CreateTemporaryDataStore(options =>
            {
                options.Schema = "schema_xxx";
            });

            dataStore.Options.Schema.Should().Be("schema_xxx");
        }

        [Fact]
        public async Task Test_SqlServerDataStore_Inits_And_Saves_Request_And_Session()
        {
            var dataStore = CreateTemporaryDataStore();
            var request = CreateTestRequest();

            await dataStore.AddRequestAsync(request);

            var requests = dataStore.GetLastRequests();
            requests.Should().HaveCount(1);
            requests[0].Id.Should().NotBe(0);
            requests[0].SessionId.Should().NotBe(0);
            requests[0].DateUtc.Should().BeCloseTo(request.DateUtc, 2.Milliseconds());
            requests[0].Path.Should().Be(request.Path);
            requests[0].QueryString.Should().Be(request.QueryString);
            requests[0].IsBot.Should().Be(request.IsBot);
            requests[0].Referrer.Should().Be(request.Referrer);
            requests[0].UtmSource.Should().Be(request.UtmSource);
            requests[0].UtmMedium.Should().Be(request.UtmMedium);
            requests[0].UtmCampaign.Should().Be(request.UtmCampaign);
            requests[0].UtmTerm.Should().Be(request.UtmTerm);
            requests[0].UtmContent.Should().Be(request.UtmContent);
            requests[0].UserName.Should().Be(request.UserName);
            requests[0].CustomData.Should().Be(request.CustomData);
            requests[0].ResponseCode.Should().Be(request.ResponseCode);
            requests[0].ResponseTime.Should().Be(request.ResponseTime);
            requests[0].ContentType.Should().Be(request.ContentType);

            var sessions = dataStore.GetLastSessions();
            sessions.Should().HaveCount(1);
            sessions[0].Id.Should().NotBe(0);
            sessions[0].HashCode.Should().NotBe(0);
            sessions[0].DateStartedUtc.Should().BeCloseTo(request.DateUtc, 2.Milliseconds());
            sessions[0].DateEndedUtc.Should().BeCloseTo(request.DateUtc, 2.Milliseconds());
            sessions[0].IsBounce.Should().BeTrue();
            sessions[0].IPAddress.Should().Be(request.IPAddress);
            sessions[0].UserAgent.Should().Be(request.UserAgent);
            sessions[0].Language.Should().Be(request.Language);
            sessions[0].Country.Should().Be(request.Country);
            sessions[0].BrowserName.Should().Be(request.BrowserName);
            sessions[0].BrowserVersion.Should().Be(request.BrowserVersion);
            sessions[0].OSName.Should().Be(request.OSName);
            sessions[0].OSVersion.Should().Be(request.OSVersion);
            sessions[0].EntryPath.Should().Be(request.Path);
            sessions[0].ExitPath.Should().Be(request.Path);
            sessions[0].Referrer.Should().Be(request.Referrer);
            sessions[0].UtmSource.Should().Be(request.UtmSource);
            sessions[0].UtmMedium.Should().Be(request.UtmMedium);
            sessions[0].UtmCampaign.Should().Be(request.UtmCampaign);
            sessions[0].UtmTerm.Should().Be(request.UtmTerm);
            sessions[0].UtmContent.Should().Be(request.UtmContent);
            sessions[0].UserName.Should().Be(request.UserName);
            sessions[0].CustomData.Should().Be(request.CustomData);
            sessions[0].Duration.Should().Be(0);
            sessions[0].RequestCount.Should().Be(1);
            requests[0].SessionId.Should().Be(sessions[0].Id);
        }

        [Fact]
        public async Task Test_SqlServerDataStore_Can_Save_Abnormally_Long_Strings()
        {
            var dataStore = CreateTemporaryDataStore();

            var longString = new string('X', 10_000);

            var request = new RequestData
            {
                DateUtc = DateTime.UtcNow,
                IPAddress = longString,
                Path = longString,
                QueryString = longString,
                UserAgent = longString,
                Language = longString,
                Country = longString,
                BrowserName = longString,
                BrowserVersion = longString,
                OSName = longString,
                OSVersion = longString,
                Referrer = longString,
                ReferrerName = longString,
                UtmSource = longString,
                UtmMedium = longString,
                UtmCampaign = longString,
                UtmTerm = longString,
                UtmContent = longString,
                UserName = longString,
                CustomData = longString,
                ResponseCode = 200,
                ResponseTime = 100,
                ContentType = longString
            };

            await dataStore.AddRequestAsync(request);

            // Strings should have been truncated
            var requests = dataStore.GetLastRequests();
            requests.Should().HaveCount(1);
            requests[0].Path.Should().HaveLength(1_000);
            requests[0].QueryString.Should().HaveLength(1_000);
            requests[0].Referrer.Should().HaveLength(1_000);
            requests[0].UtmSource.Should().HaveLength(50);
            requests[0].UtmMedium.Should().HaveLength(50);
            requests[0].UtmCampaign.Should().HaveLength(50);
            requests[0].UtmTerm.Should().HaveLength(50);
            requests[0].UtmContent.Should().HaveLength(50);
            requests[0].UserName.Should().HaveLength(50);
            requests[0].CustomData.Should().HaveLength(10_000);
            requests[0].ContentType.Should().HaveLength(50);

            // Strings should have been truncated
            var sessions = dataStore.GetLastSessions();
            sessions.Should().HaveCount(1);
            sessions[0].IPAddress.Should().HaveLength(45);
            sessions[0].UserAgent.Should().HaveLength(1_000);
            sessions[0].Language.Should().HaveLength(5);
            sessions[0].Country.Should().HaveLength(2);
            sessions[0].BrowserName.Should().HaveLength(50);
            sessions[0].BrowserVersion.Should().HaveLength(50);
            sessions[0].OSName.Should().HaveLength(50);
            sessions[0].OSVersion.Should().HaveLength(50);
            sessions[0].EntryPath.Should().HaveLength(1_000);
            sessions[0].ExitPath.Should().HaveLength(1_000);
            sessions[0].Referrer.Should().HaveLength(1_000);
            sessions[0].UtmSource.Should().HaveLength(50);
            sessions[0].UtmMedium.Should().HaveLength(50);
            sessions[0].UtmCampaign.Should().HaveLength(50);
            sessions[0].UtmTerm.Should().HaveLength(50);
            sessions[0].UtmContent.Should().HaveLength(50);
            sessions[0].UserName.Should().HaveLength(50);
            sessions[0].CustomData.Should().HaveLength(10_000);
        }

        [Fact]
        public async Task Test_SqlServerDataStore_Assigns_Requests_To_Sessions_Based_On_Browser()
        {
            // Arrange
            var start = DateTime.UtcNow.AddHours(-12);
            var dataStore = CreateTemporaryDataStore();
            var inputs = new[] {
                CreateTestRequest(dateUtc: start.AddMinutes(10), browserName: "Chrome"),
                CreateTestRequest(dateUtc: start.AddMinutes(20), browserName: "Chrome"),
                CreateTestRequest(dateUtc: start.AddMinutes(30), browserName: "Firefox"),
                CreateTestRequest(dateUtc: start.AddMinutes(40), browserName: "Chrome"),
                CreateTestRequest(dateUtc: start.AddMinutes(50), browserName: "Firefox"),
            };

            // Act
            foreach (var request in inputs)
                await dataStore.AddRequestAsync(request);

            // Assert: check sessions
            // Session[0] should be Firefox (last session returned first)
            // Session[1] should be Chrome
            var sessions = dataStore.GetLastSessions();
            sessions.Should().HaveCount(2);
            sessions[0].RequestCount.Should().Be(2);
            sessions[0].Duration.Should().Be(20 * 60);
            sessions[1].RequestCount.Should().Be(3);
            sessions[1].Duration.Should().Be(30 * 60);

            // Assert: check requests
            var requests = dataStore.GetLastRequests();
            requests.Should().HaveCount(5);
            requests.Where(x => x.SessionId == sessions[0].Id).Should().AllSatisfy(x => x.SessionId.Should().Be(sessions[0].Id));
            requests.Where(x => x.SessionId == sessions[1].Id).Should().AllSatisfy(x => x.SessionId.Should().Be(sessions[1].Id));
        }

        [Fact]
        public async Task Test_SqlServerDataStore_Creates_Reuses_Session_If_Less_Than_30_Minutes()
        {
            // Arrange
            var start = DateTime.UtcNow;
            var dataStore = CreateTemporaryDataStore();

            // Act
            for (var i = 0; i < 10; i++)
                await dataStore.AddRequestAsync(CreateTestRequest(dateUtc: start.AddMinutes(i * 10)));

            // Assert: check sessions
            var sessions = dataStore.GetLastSessions();
            sessions.Should().HaveCount(1);
            sessions[0].RequestCount.Should().Be(10);

            // Assert: check requests
            var requests = dataStore.GetLastRequests();
            requests.Should().HaveCount(10);
            requests.Should().AllSatisfy(x => x.SessionId.Should().Be(sessions[0].Id));
        }

        [Fact]
        public async Task Test_SqlServerDataStore_Creates_Reuses_Session_If_Less_Than_30_Minutes_IPv6()
        {
            // Arrange
            var start = DateTime.UtcNow;
            var dataStore = CreateTemporaryDataStore();

            // Act
            for (var i = 0; i < 10; i++)
                await dataStore.AddRequestAsync(CreateTestRequest(dateUtc: start.AddMinutes(i * 10), ipAddress: "2345:0425:2CA1:0000:0000:0567:5673:23b5"));

            // Assert: check sessions
            var sessions = dataStore.GetLastSessions();
            sessions.Should().HaveCount(1);
            sessions[0].RequestCount.Should().Be(10);

            // Assert: check requests
            var requests = dataStore.GetLastRequests();
            requests.Should().HaveCount(10);
            requests.Should().AllSatisfy(x => x.SessionId.Should().Be(sessions[0].Id));
        }

        [Fact]
        public async Task Test_SqlServerDataStore_Reusing_Sessions_Works_With_Missing_Data()
        {
            // Arrange
            var start = DateTime.UtcNow.AddHours(-12);
            var dataStore = CreateTemporaryDataStore();
            var inputs = new[] {
                CreateTestRequest(dateUtc: start.AddMinutes(10), browserName: null, browserVersion: null),
                CreateTestRequest(dateUtc: start.AddMinutes(20), browserName: null, browserVersion: null),
                CreateTestRequest(dateUtc: start.AddMinutes(30), browserName: null, browserVersion: null),
                CreateTestRequest(dateUtc: start.AddMinutes(40), browserName: null, browserVersion: null),
                CreateTestRequest(dateUtc: start.AddMinutes(50), browserName: null, browserVersion: null),
            };

            // Act
            foreach (var request in inputs)
                await dataStore.AddRequestAsync(request);

            // Assert: check sessions
            var sessions = dataStore.GetLastSessions();
            sessions.Should().HaveCount(1);
            sessions[0].RequestCount.Should().Be(5);
            sessions[0].Duration.Should().Be(40 * 60);

            // Assert: check requests
            var requests = dataStore.GetLastRequests();
            requests.Should().HaveCount(5);
            requests.Should().AllSatisfy(x => x.SessionId.Should().Be(sessions[0].Id));
        }

        [Fact]
        public async Task Test_SqlServerDataStore_Creates_New_Session_After_30_Minutes()
        {
            // Arrange
            var start = DateTime.UtcNow;
            var dataStore = CreateTemporaryDataStore();
            var inputs = new[] {
                CreateTestRequest(dateUtc: start.AddMinutes(10), browserName: "Chrome"),    // Session 1
                CreateTestRequest(dateUtc: start.AddMinutes(20), browserName: "Firefox"),   // Session 2
                CreateTestRequest(dateUtc: start.AddMinutes(30), browserName: "Chrome"),    // Session 1
                CreateTestRequest(dateUtc: start.AddMinutes(40), browserName: "Firefox"),   // Session 2
                CreateTestRequest(dateUtc: start.AddMinutes(50), browserName: "Firefox"),   // Session 2
                CreateTestRequest(dateUtc: start.AddMinutes(60), browserName: "Chrome"),    // Session 3 <-- new session
            };

            // Act
            foreach (var request in inputs)
                await dataStore.AddRequestAsync(request);

            // Assert: check sessions
            // Session[0] should be Session 3 (last session returned first)
            var sessions = dataStore.GetLastSessions(); // TODO: use GetAllSessions instead and avoid reversing the order?
            sessions.Should().HaveCount(3);
            sessions[0].RequestCount.Should().Be(1);
            sessions[0].Duration.Should().Be(0);
            sessions[1].RequestCount.Should().Be(3);
            sessions[1].Duration.Should().Be(30 * 60);
            sessions[2].RequestCount.Should().Be(2);
            sessions[2].Duration.Should().Be(20 * 60);

            // Assert: check requests
            var requests = dataStore.GetLastRequests();
            requests.Should().HaveCount(6);
            requests.First().SessionId.Should().Be(sessions[0].Id);
        }

        [Fact]
        public async Task Test_SqlServerDataStore_Can_Read_Request_With_Null_Fields()
        {
            var dataStore = CreateTemporaryDataStore();
            var request = new RequestData
            {
                DateUtc = DateTime.UtcNow,
                IPAddress = "127.0.0.1",
                Path = "/test"
            };

            await dataStore.AddRequestAsync(request);

            var requests = dataStore.GetLastRequests();
            requests.Should().HaveCount(1);
            requests[0].Id.Should().NotBe(0);
            requests[0].SessionId.Should().NotBe(0);
            requests[0].DateUtc.Should().BeCloseTo(request.DateUtc, 2.Milliseconds());
            requests[0].Path.Should().Be(request.Path);
            requests[0].QueryString.Should().Be(request.QueryString);
            requests[0].IsBot.Should().Be(request.IsBot);
            requests[0].Referrer.Should().Be(request.Referrer);
            requests[0].UtmSource.Should().Be(request.UtmSource);
            requests[0].UtmMedium.Should().Be(request.UtmMedium);
            requests[0].UtmCampaign.Should().Be(request.UtmCampaign);
            requests[0].UtmTerm.Should().Be(request.UtmTerm);
            requests[0].UtmContent.Should().Be(request.UtmContent);
            requests[0].UserName.Should().Be(request.UserName);
            requests[0].CustomData.Should().Be(request.CustomData);
            requests[0].ResponseCode.Should().Be(request.ResponseCode);
            requests[0].ResponseTime.Should().Be(request.ResponseTime);
            requests[0].ContentType.Should().Be(request.ContentType);
        }

        [Fact]
        public async Task Test_SqlServerDataStore_Can_Ingest_Requests_In_Time_Order()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();
            var request1 = CreateTestRequest(dateUtc: DateTime.UtcNow.AddSeconds(-2), path: "/entry");
            var request2 = CreateTestRequest(dateUtc: DateTime.UtcNow.AddSeconds(-1), path: "/exit");

            // Act
            // We are recording the oldest request first
            await dataStore.AddRequestAsync(request1);
            await dataStore.AddRequestAsync(request2);

            // Assert
            var sessions = dataStore.GetLastSessions();
            sessions.Should().HaveCount(1);
            sessions[0].DateStartedUtc.Should().BeCloseTo(request1.DateUtc, 2.Milliseconds());
            sessions[0].DateEndedUtc.Should().BeCloseTo(request2.DateUtc, 2.Milliseconds());
            sessions[0].Duration.Should().Be(1);
            sessions[0].RequestCount.Should().Be(2);
            sessions[0].EntryPath.Should().Be(request1.Path);
            sessions[0].ExitPath.Should().Be(request2.Path);
        }

        [Fact]
        public async Task Test_SqlServerDataStore_Can_Ingest_Requests_In_Any_Order()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();
            var request1 = CreateTestRequest(dateUtc: DateTime.UtcNow.AddSeconds(-2), path: "/entry");
            var request2 = CreateTestRequest(dateUtc: DateTime.UtcNow.AddSeconds(-1), path: "/exit");

            // Act
            // We are recording the most recent request first
            await dataStore.AddRequestAsync(request2);
            await dataStore.AddRequestAsync(request1);

            // Assert
            var sessions = dataStore.GetLastSessions();
            sessions.Should().HaveCount(1);
            sessions[0].DateStartedUtc.Should().BeCloseTo(request1.DateUtc, 2.Milliseconds());
            sessions[0].DateEndedUtc.Should().BeCloseTo(request2.DateUtc, 2.Milliseconds());
            sessions[0].Duration.Should().Be(1);
            sessions[0].RequestCount.Should().Be(2);
            sessions[0].EntryPath.Should().Be(request1.Path);
            sessions[0].ExitPath.Should().Be(request2.Path);
        }

        [Fact(Skip = "Private method has moved somewhere else")]
        public void Test_SqlServerDataStore_FindSession_Returns_Null_When_No_Session_Exists()
        {
            // Arrange
            var request = CreateTestRequest();
            var dataStore = CreateTemporaryDataStore();

            // Act
            var session = CallPrivateMethod<SqlServerDataStore, Session>(dataStore, "FindSession", request);

            // Assert
            session.Should().BeNull();
        }

        [Fact(Skip = "Private method has moved somewhere else")]
        public async Task Test_SqlServerDataStore_FindSession_Returns_Session_When_Session_Found()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();
            var request1 = CreateTestRequest(dateUtc: DateTime.UtcNow.AddMinutes(-10));
            var request2 = CreateTestRequest();
            await dataStore.AddRequestAsync(request1);

            // Act
            var session = CallPrivateMethod<SqlServerDataStore, Session>(dataStore, "FindSession", request2);

            // Assert
            session.Should().NotBeNull();
        }

        [Fact(Skip = "Private method has moved somewhere else")]
        public async Task Test_SqlServerDataStore_FindSession_Returns_Null_When_No_Recent_Session()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();
            var request1 = CreateTestRequest(dateUtc: DateTime.UtcNow.AddMinutes(-60));
            var request2 = CreateTestRequest();
            await dataStore.AddRequestAsync(request1);

            // Act
            var session = CallPrivateMethod<SqlServerDataStore, Session>(dataStore, "FindSession", request2);

            // Assert
            session.Should().BeNull();
        }

        [Fact]
        public void Test_SqlServerDataStore_Inits_With_Empty_Geocoding_Table()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();

            // Act
            var empty = dataStore.NeedsGeocodingDatabase();
            var country = dataStore.GetCountryCodeForIPAddress(IPAddress.Parse("127.0.0.1"));

            // Assert
            empty.Should().BeTrue();
            country.Should().BeNull();
        }

        [Fact]
        public void Test_SqlServerDataStore_Can_Import_And_Use_Geocoding_Data()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();

            // Read sample data
            // TODO: put this somewhere in the lib?
            var lines = File.ReadAllLines("Files/Geocoding-Data-Sample.csv");
            var data = new List<GeolocRange>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3)
                    {
                        var start = IPAddress.Parse(parts[0]);
                        var end = IPAddress.Parse(parts[1]);
                        var country = parts[2];

                        data.Add(new GeolocRange(start, end, country.ToUpper()));
                    }
                }
            }

            // Act
            dataStore.ImportGeocodingData(data);
            var empty = dataStore.NeedsGeocodingDatabase();
            var country1 = dataStore.GetCountryCodeForIPAddress(IPAddress.Parse("127.0.0.1"));
            var country2 = dataStore.GetCountryCodeForIPAddress(IPAddress.Parse("1.4.128.0"));
            var country3 = dataStore.GetCountryCodeForIPAddress(IPAddress.Parse("1.32.219.125"));
            var country4 = dataStore.GetCountryCodeForIPAddress(IPAddress.Parse("2001:420:93:c4df:ea22:dace:9dff:0931"));
            var country5 = dataStore.GetCountryCodeForIPAddress(IPAddress.Parse("2001:420:c0c4:f392:ee18:d2e0:6014:a3f1"));

            // Assert
            empty.Should().BeFalse();
            country1.Should().BeNull();
            country2.Should().Be("TH");
            country3.Should().Be("SG");
            country4.Should().Be("AU");
            country4.Should().Be("AU");
            country5.Should().Be("US");
        }

        [Fact]
        public void Test_SqlServerDataStore_Can_Save_And_Load_Settings()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();
            var settings = new Settings
            {
                SchemaVersion = 18,
                GeocodingDataVersion = null
            };

            // Act
            dataStore.SaveSettings(settings);
            var saved = dataStore.GetSettings();

            // Assert
            saved.SchemaVersion.Should().Be(18);
            saved.GeocodingDataVersion.Should().Be(null);
        }

        [Fact]
        public void Test_SqlServerDataStore_Can_Save_And_Load_Individual_Settings()
        {
            // Arrange
            var dataStore = CreateTemporaryDataStore();

            // Act
            dataStore.SaveSetting("Test 1", 0);
            dataStore.SaveSetting("Test 1", 1);
            dataStore.SaveSetting("Test 2", 2);
            var test1 = dataStore.GetSetting<int>("Test 1");
            var test2 = dataStore.GetSetting<int>("Test 2");

            // Assert
            test1.Should().Be(1);
            test2.Should().Be(2);
        }

        private static TReturn CallPrivateMethod<TInstance, TReturn>(TInstance instance, string methodName, params object[] parameters)
        {
            var type = instance!.GetType();
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance) ??
                throw new InvalidOperationException($"Method {methodName} not found on type {type.Name}");

            return (TReturn)method.Invoke(instance, parameters)!;
        }

        public void Dispose()
        {
            var dataStore1 = CreateTemporaryDataStore();
            dataStore1.DropSchema();

            var dataStore2 = CreateTemporaryDataStore(options => options.Schema = "schema_xxx");
            dataStore2.DropSchema();
        }
    }
}
