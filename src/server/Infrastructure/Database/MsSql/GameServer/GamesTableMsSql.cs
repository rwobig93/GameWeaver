using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class GamesTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "Games";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(GamesTableMsSql).GetDbScriptsFromClass();

    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 5,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                    [FriendlyName] NVARCHAR(128) NOT NULL,
                    [SteamName] NVARCHAR(128) NOT NULL,
                    [SteamGameId] INT NULL,
                    [SteamToolId] INT NULL,
                    [DefaultGameProfileId] UNIQUEIDENTIFIER NOT NULL,
                    [LatestBuildVersion] NVARCHAR(128) NOT NULL,
                    [UrlBackground] NVARCHAR(256) NOT NULL,
                    [UrlLogo] NVARCHAR(256) NOT NULL,
                    [UrlLogoSmall] NVARCHAR(256) NOT NULL,
                    [UrlWebsite] NVARCHAR(256) NOT NULL,
                    [ControllerSupport] NVARCHAR(128) NOT NULL,
                    [DescriptionShort] NVARCHAR(256) NOT NULL,
                    [DescriptionLong] NVARCHAR(4000) NOT NULL,
                    [DescriptionAbout] NVARCHAR(2048) NOT NULL,
                    [PriceInitial] NVARCHAR(128) NOT NULL,
                    [PriceCurrent] NVARCHAR(128) NOT NULL,
                    [PriceDiscount] INT NOT NULL,
                    [MetaCriticScore] INT NOT NULL,
                    [UrlMetaCriticPage] NVARCHAR(128) NOT NULL,
                    [RequirementsPcMinimum] NVARCHAR(128) NOT NULL,
                    [RequirementsPcRecommended] NVARCHAR(128) NOT NULL,
                    [RequirementsMacMinimum] NVARCHAR(128) NOT NULL,
                    [RequirementsMacRecommended] NVARCHAR(128) NOT NULL,
                    [RequirementsLinuxMinimum] NVARCHAR(128) NOT NULL,
                    [RequirementsLinuxRecommended] NVARCHAR(128) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [CreatedOn] DATETIME2 NOT NULL,
                    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
                    [LastModifiedOn] DATETIME2 NULL,
                    [IsDeleted] BIT NOT NULL,
                    [DeletedOn] DATETIME2 NULL,
                    [SupportsWindows] INT NOT NULL,
                    [SupportsLinux] INT NOT NULL,
                    [SupportsMac] INT NOT NULL,
                    [SourceType] INT NOT NULL,
                    [ManualFileRecordId] UNIQUEIDENTIFIER NULL,
                    [ManualVersionUrlCheck] NVARCHAR(1024) NULL,
                    [ManualVersionUrlCheckPath] NVARCHAR(1024) NULL,
                    [ManualVersionUrlDownload] NVARCHAR(1024) NULL,
                    [ManualGameClientInstallUrl] NVARCHAR(1024) NULL
                )
            end"
    };

    public static readonly SqlStoredProcedure Delete = new()
    {
        Table = Table,
        Action = "Delete",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Delete]
                @Id UNIQUEIDENTIFIER,
                @DeletedBy UNIQUEIDENTIFIER,
                @DeletedOn DATETIME2
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET IsDeleted = 1, DeletedOn = @DeletedOn, LastModifiedBy = @DeletedBy
                WHERE Id = @Id;
            end"
    };

    public static readonly SqlStoredProcedure GetAll = new()
    {
        Table = Table,
        Action = "GetAll",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAll]
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0
                ORDER BY g.FriendlyName ASC;
            end"
    };

    public static readonly SqlStoredProcedure GetAllPaginated = new()
    {
        Table = Table,
        Action = "GetAllPaginated",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllPaginated]
                @Offset INT,
                @PageSize INT
            AS
            begin
                SELECT COUNT(*) OVER() AS TotalCount, g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0
                ORDER BY g.FriendlyName ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };

    public static readonly SqlStoredProcedure GetById = new()
    {
        Table = Table,
        Action = "GetById",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetById]
                @Id UNIQUEIDENTIFIER
            AS
            begin
                SELECT TOP 1 g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.Id = @Id
                ORDER BY g.CreatedOn DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetBySteamName = new()
    {
        Table = Table,
        Action = "GetBySteamName",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetBySteamName]
                @SteamName NVARCHAR(128)
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.SteamName = @SteamName AND g.IsDeleted = 0
                ORDER BY g.CreatedOn DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetByFriendlyName = new()
    {
        Table = Table,
        Action = "GetByFriendlyName",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByFriendlyName]
                @FriendlyName NVARCHAR(128)
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.FriendlyName = @FriendlyName AND g.IsDeleted = 0
                ORDER BY g.CreatedOn DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetBySteamGameId = new()
    {
        Table = Table,
        Action = "GetBySteamGameId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetBySteamGameId]
                @SteamGameId INT
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.SteamGameId = @SteamGameId AND g.IsDeleted = 0
                ORDER BY g.CreatedOn DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetBySteamToolId = new()
    {
        Table = Table,
        Action = "GetBySteamToolId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetBySteamToolId]
                @SteamToolId INT
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.SteamToolId = @SteamToolId AND g.IsDeleted = 0
                ORDER BY g.CreatedOn DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetBySourceType = new()
    {
        Table = Table,
        Action = "GetBySourceType",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetBySourceType]
                @SourceType INT
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.SourceType = @SourceType AND g.IsDeleted = 0
                ORDER BY g.FriendlyName ASC;
            end"
    };

    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @Id UNIQUEIDENTIFIER,
                @FriendlyName NVARCHAR(128),
                @SteamName NVARCHAR(128),
                @SteamGameId INT,
                @SteamToolId INT,
                @DefaultGameProfileId UNIQUEIDENTIFIER,
                @LatestBuildVersion NVARCHAR(128),
                @UrlBackground NVARCHAR(258),
                @UrlLogo NVARCHAR(256),
                @UrlLogoSmall NVARCHAR(256),
                @UrlWebsite NVARCHAR(256),
                @ControllerSupport NVARCHAR(128),
                @DescriptionShort NVARCHAR(256),
                @DescriptionLong NVARCHAR(4000),
                @DescriptionAbout NVARCHAR(2048),
                @PriceInitial NVARCHAR(128),
                @PriceCurrent NVARCHAR(128),
                @PriceDiscount INT,
                @MetaCriticScore INT,
                @UrlMetaCriticPage NVARCHAR(128),
                @RequirementsPcMinimum NVARCHAR(128),
                @RequirementsPcRecommended NVARCHAR(128),
                @RequirementsMacMinimum NVARCHAR(128),
                @RequirementsMacRecommended NVARCHAR(128),
                @RequirementsLinuxMinimum NVARCHAR(128),
                @RequirementsLinuxRecommended NVARCHAR(128),
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn DATETIME2,
                @LastModifiedBy UNIQUEIDENTIFIER,
                @LastModifiedOn DATETIME2,
                @IsDeleted BIT,
                @SupportsWindows INT,
                @SupportsLinux INT,
                @SupportsMac INT,
                @SourceType INT,
                @ManualFileRecordId UNIQUEIDENTIFIER,
                @ManualVersionUrlCheck NVARCHAR(1024),
                @ManualVersionUrlDownload NVARCHAR(1024)
            AS
            begin
                INSERT into dbo.[{Table.TableName}]  (Id, FriendlyName, SteamName, SteamGameId, SteamToolId, DefaultGameProfileId, LatestBuildVersion, UrlBackground, UrlLogo,
                                                      UrlLogoSmall, UrlWebsite, ControllerSupport, DescriptionShort, DescriptionLong, DescriptionAbout, PriceInitial, PriceCurrent,
                                                      PriceDiscount, MetaCriticScore, UrlMetaCriticPage, RequirementsPcMinimum, RequirementsPcRecommended, RequirementsMacMinimum,
                                                      RequirementsMacRecommended, RequirementsLinuxMinimum, RequirementsLinuxRecommended, CreatedBy, CreatedOn, LastModifiedBy,
                                                      LastModifiedOn, IsDeleted, SupportsWindows, SupportsLinux, SupportsMac, SourceType, ManualFileRecordId, ManualVersionUrlCheck,
                                                      ManualVersionUrlDownload)
                OUTPUT INSERTED.Id
                VALUES (@Id, @FriendlyName, @SteamName, @SteamGameId, @SteamToolId, @DefaultGameProfileId, @LatestBuildVersion, @UrlBackground, @UrlLogo, @UrlLogoSmall, @UrlWebsite,
                        @ControllerSupport, @DescriptionShort, @DescriptionLong, @DescriptionAbout, @PriceInitial, @PriceCurrent, @PriceDiscount, @MetaCriticScore,
                        @UrlMetaCriticPage, @RequirementsPcMinimum, @RequirementsPcRecommended, @RequirementsMacMinimum, @RequirementsMacRecommended, @RequirementsLinuxMinimum,
                        @RequirementsLinuxRecommended, @CreatedBy, @CreatedOn, @LastModifiedBy, @LastModifiedOn, @IsDeleted, @SupportsWindows, @SupportsLinux, @SupportsMac,
                        @SourceType, @ManualFileRecordId, @ManualVersionUrlCheck, @ManualVersionUrlDownload);
            end"
    };

    public static readonly SqlStoredProcedure Search = new()
    {
        Table = Table,
        Action = "Search",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Search]
                @SearchTerm NVARCHAR(256)
            AS
            begin
                SET nocount on;
                
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0
                    AND (g.Id LIKE '%' + @SearchTerm + '%'
                    OR g.FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR g.SteamName LIKE '%' + @SearchTerm + '%'
                    OR g.SteamGameId LIKE '%' + @SearchTerm + '%'
                    OR g.SteamToolId LIKE '%' + @SearchTerm + '%'
                    OR g.LatestBuildVersion LIKE '%' + @SearchTerm + '%'
                    OR g.DescriptionShort LIKE '%' + @SearchTerm + '%'
                    OR g.ManualFileRecordId LIKE '%' + @SearchTerm + '%')
                ORDER BY g.FriendlyName ASC;
            end"
    };

    public static readonly SqlStoredProcedure SearchPaginated = new()
    {
        Table = Table,
        Action = "SearchPaginated",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_SearchPaginated]
                @SearchTerm NVARCHAR(256),
                @Offset INT,
                @PageSize INT
            AS
            begin
                SET nocount on;
                
                SELECT COUNT(*) OVER() AS TotalCount, g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0
                    AND (g.Id LIKE '%' + @SearchTerm + '%'
                    OR g.FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR g.SteamName LIKE '%' + @SearchTerm + '%'
                    OR g.SteamGameId LIKE '%' + @SearchTerm + '%'
                    OR g.SteamToolId LIKE '%' + @SearchTerm + '%'
                    OR g.LatestBuildVersion LIKE '%' + @SearchTerm + '%'
                    OR g.DescriptionShort LIKE '%' + @SearchTerm + '%'
                    OR g.ManualFileRecordId LIKE '%' + @SearchTerm + '%')
                ORDER BY g.FriendlyName ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };

    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Update]
                @Id UNIQUEIDENTIFIER,
                @FriendlyName NVARCHAR(128) = null,
                @SteamName NVARCHAR(128) = null,
                @SteamGameId INT = null,
                @SteamToolId INT = null,
                @DefaultGameProfileId UNIQUEIDENTIFIER = null,
                @LatestBuildVersion NVARCHAR(128) = null,
                @UrlBackground NVARCHAR(258) = null,
                @UrlLogo NVARCHAR(256) = null,
                @UrlLogoSmall NVARCHAR(256) = null,
                @UrlWebsite NVARCHAR(256) = null,
                @ControllerSupport NVARCHAR(128) = null,
                @DescriptionShort NVARCHAR(256) = null,
                @DescriptionLong NVARCHAR(4000) = null,
                @DescriptionAbout NVARCHAR(2048) = null,
                @PriceInitial NVARCHAR(128) = null,
                @PriceCurrent NVARCHAR(128) = null,
                @PriceDiscount INT = null,
                @MetaCriticScore INT = null,
                @UrlMetaCriticPage NVARCHAR(128) = null,
                @RequirementsPcMinimum NVARCHAR(128) = null,
                @RequirementsPcRecommended NVARCHAR(128) = null,
                @RequirementsMacMinimum NVARCHAR(128) = null,
                @RequirementsMacRecommended NVARCHAR(128) = null,
                @RequirementsLinuxMinimum NVARCHAR(128) = null,
                @RequirementsLinuxRecommended NVARCHAR(128) = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn DATETIME2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn DATETIME2 = null,
                @IsDeleted BIT = null,
                @DeletedOn DATETIME2 = null,
                @SupportsWindows BIT = null,
                @SupportsLinux BIT = null,
                @SupportsMac BIT = null,
                @SourceType INT = null,
                @ManualFileRecordId UNIQUEIDENTIFIER = null,
                @ManualVersionUrlCheck NVARCHAR(1024) = null,
                @ManualVersionUrlDownload NVARCHAR(1024) = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET FriendlyName = COALESCE(@FriendlyName, FriendlyName), SteamName = COALESCE(@SteamName, SteamName),
                    SteamGameId = COALESCE(@SteamGameId, SteamGameId), SteamToolId = COALESCE(@SteamToolId, SteamToolId),
                    DefaultGameProfileId = COALESCE(@DefaultGameProfileId, DefaultGameProfileId), UrlBackground = COALESCE(@UrlBackground, UrlBackground),
                    LatestBuildVersion = COALESCE(@LatestBuildVersion, LatestBuildVersion), UrlLogo = COALESCE(@UrlLogo, UrlLogo),
                    UrlLogoSmall = COALESCE(@UrlLogoSmall, UrlLogoSmall), UrlWebsite = COALESCE(@UrlWebsite, UrlWebsite),
                    ControllerSupport = COALESCE(@ControllerSupport, ControllerSupport), DescriptionShort = COALESCE(@DescriptionShort, DescriptionShort),
                    DescriptionLong = COALESCE(@DescriptionLong, DescriptionLong), DescriptionAbout = COALESCE(@DescriptionAbout, DescriptionAbout),
                    PriceInitial = COALESCE(@PriceInitial, PriceInitial), PriceCurrent = COALESCE(@PriceCurrent, PriceCurrent),
                    PriceDiscount = COALESCE(@PriceDiscount, PriceDiscount), MetaCriticScore = COALESCE(@MetaCriticScore, MetaCriticScore),
                    UrlMetaCriticPage = COALESCE(@UrlMetaCriticPage, UrlMetaCriticPage), RequirementsPcMinimum = COALESCE(@RequirementsPcMinimum, RequirementsPcMinimum),
                    RequirementsPcRecommended = COALESCE(@RequirementsPcRecommended, RequirementsPcRecommended),
                    RequirementsMacMinimum = COALESCE(@RequirementsMacMinimum, RequirementsMacMinimum),
                    RequirementsMacRecommended = COALESCE(@RequirementsMacRecommended, RequirementsMacRecommended),
                    RequirementsLinuxMinimum = COALESCE(@RequirementsLinuxMinimum, RequirementsLinuxMinimum),
                    RequirementsLinuxRecommended = COALESCE(@RequirementsLinuxRecommended, RequirementsLinuxRecommended), CreatedBy = COALESCE(@CreatedBy, CreatedBy),
                    CreatedOn = COALESCE(@CreatedOn, CreatedOn), LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy),
                    LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn), IsDeleted = COALESCE(@IsDeleted, IsDeleted), DeletedOn = COALESCE(@DeletedOn, DeletedOn),
                    SupportsWindows = COALESCE(@SupportsWindows, SupportsWindows), SupportsLinux = COALESCE(@SupportsLinux, SupportsLinux),
                    SupportsMac = COALESCE(@SupportsMac, SupportsMac), SourceType = COALESCE(@SourceType, SourceType),
                    ManualFileRecordId = COALESCE(@ManualFileRecordId, ManualFileRecordId), ManualVersionUrlCheck = COALESCE(@ManualVersionUrlCheck, ManualVersionUrlCheck),
                    ManualVersionUrlDownload = COALESCE(@ManualVersionUrlDownload, ManualVersionUrlDownload)
                WHERE Id = @Id;
            end"
    };
}