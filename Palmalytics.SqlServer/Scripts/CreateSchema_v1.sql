CREATE TABLE [{schema}].[{settingsTable}] (
	[Name] NVARCHAR(50) NOT NULL,
	[Value] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_{settingsTable}] PRIMARY KEY ([Name])
)

CREATE TABLE [{schema}].[{geocodingTable}] (
	[Start] VARBINARY(16) NOT NULL,
	[End] VARBINARY(16) NOT NULL,
	[Country] CHAR(2) NOT NULL,
	[IPVersion] TINYINT NOT NULL,
    CONSTRAINT [PK_Geocoding] PRIMARY KEY ([Start], [IPVersion])
)

CREATE TABLE [{schema}].[{sessionsTable}]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
	[HashCode] INT NOT NULL,
	[DateStartedUtc] DATETIME NOT NULL,
	[DateEndedUtc] DATETIME NOT NULL,
	[IPAddress] NVARCHAR(45) NOT NULL,
	[UserAgent] NVARCHAR(1000) NULL,
	[Language] NVARCHAR(5) NULL,
	[Country] CHAR(2) NULL,
	[BrowserName] NVARCHAR(50) NULL,
	[BrowserVersion] NVARCHAR(50) NULL,
	[OSName] NVARCHAR(50) NULL,
	[OSVersion] NVARCHAR(50) NULL,
	[EntryPath] NVARCHAR(1000) NOT NULL,
	[ExitPath] NVARCHAR(1000) NOT NULL,
	[IsBounce] BIT NOT NULL,
	[Referrer] NVARCHAR(1000) NULL,
	[UtmSource] NVARCHAR(50) NULL,
	[UtmMedium] NVARCHAR(50) NULL,
	[UtmCampaign] NVARCHAR(50) NULL,
	[UtmTerm] NVARCHAR(50) NULL,
	[UtmContent] NVARCHAR(50) NULL,
	[UserName] NVARCHAR(50) NULL,
	[CustomData] NVARCHAR(MAX) NULL,
	[Duration] INT NOT NULL,
	[RequestCount] INT NOT NULL,
	[ReferrerName] NVARCHAR(50) NULL,
	[Sampling10] AS (CONVERT([BIT], CASE WHEN [Id] % 10 = 0 THEN 1 ELSE 0 END)) PERSISTED NOT NULL,
	[Sampling100] AS (CONVERT([BIT], CASE WHEN [Id] % 100 = 0 THEN 1 ELSE 0 END)) PERSISTED NOT NULL,
	[Sampling1000] AS (CONVERT([BIT], CASE WHEN [Id] % 1000 = 0 THEN 1 ELSE 0 END)) PERSISTED NOT NULL,
    CONSTRAINT [PK_{sessionsTable}] PRIMARY KEY (Id),
    INDEX IX_DateStartedUtc (DateStartedUtc),
    INDEX IX_DateEndedUtc (DateEndedUtc),
    INDEX IX_HashCode (HashCode),
    INDEX IX_Sampling10 (Sampling10),
    INDEX IX_Sampling100 (Sampling100),
    INDEX IX_Sampling1000 (Sampling1000)
)

CREATE TABLE [{schema}].[{requestsTable}]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
	[SessionId] BIGINT NULL,
	[DateUtc] DATETIME NOT NULL,
	[Path] NVARCHAR(1000) NOT NULL,
	[QueryString] NVARCHAR(1000) NULL,
	[Referrer] NVARCHAR(1000) NULL,
	[UtmSource] NVARCHAR(50) NULL,
	[UtmMedium] NVARCHAR(50) NULL,
	[UtmCampaign] NVARCHAR(50) NULL,
	[UtmTerm] NVARCHAR(50) NULL,
	[UtmContent] NVARCHAR(50) NULL,
	[UserName] NVARCHAR(50) NULL,
	[CustomData] NVARCHAR(MAX) NULL,
	[ResponseCode] INT NULL,
	[ResponseTime] INT NULL,
	[ContentType] NVARCHAR(50) NULL,
	[Sampling10] AS (CONVERT([BIT], CASE WHEN [Id] % 10 = 0 THEN 1 ELSE 0 END)) PERSISTED NOT NULL,
	[Sampling100] AS (CONVERT([BIT], CASE WHEN [Id] % 100 = 0 THEN 1 ELSE 0 END)) PERSISTED NOT NULL,
	[Sampling1000] AS (CONVERT([BIT], CASE WHEN [Id] % 1000 = 0 THEN 1 ELSE 0 END)) PERSISTED NOT NULL,
    CONSTRAINT [PK_{requestsTable}] PRIMARY KEY (Id),
    CONSTRAINT [FK_{requestsTable}_{sessionsTable}] FOREIGN KEY (SessionId) REFERENCES [{schema}].[{sessionsTable}] (Id) ON DELETE CASCADE,
    INDEX IX_DateUtc (DateUtc),
    INDEX IX_Sampling10 (Sampling10),
    INDEX IX_Sampling100 (Sampling100),
    INDEX IX_Sampling1000 (Sampling1000),
	INDEX IX_SessionId (SessionId)
)

INSERT INTO [{schema}].[{settingsTable}] VALUES ('SchemaVersion', '1')
