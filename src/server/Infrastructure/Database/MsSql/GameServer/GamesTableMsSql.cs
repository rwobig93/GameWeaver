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
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [FriendlyName] NVARCHAR(128) NOT NULL,
                    [SteamName] NVARCHAR(128) NOT NULL,
                    [SteamGameId] int NULL,
                    [SteamToolId] int NULL,
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
                    [PriceDiscount] int NOT NULL,
                    [MetaCriticScore] int NOT NULL,
                    [UrlMetaCriticPage] NVARCHAR(128) NOT NULL,
                    [RequirementsPcMinimum] NVARCHAR(128) NOT NULL,
                    [RequirementsPcRecommended] NVARCHAR(128) NOT NULL,
                    [RequirementsMacMinimum] NVARCHAR(128) NOT NULL,
                    [RequirementsMacRecommended] NVARCHAR(128) NOT NULL,
                    [RequirementsLinuxMinimum] NVARCHAR(128) NOT NULL,
                    [RequirementsLinuxRecommended] NVARCHAR(128) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [CreatedOn] datetime2 NOT NULL,
                    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
                    [LastModifiedOn] datetime2 NULL,
                    [IsDeleted] BIT NOT NULL,
                    [DeletedOn] datetime2 NULL,
                    [SupportsWindows] int NOT NULL,
                    [SupportsLinux] int NOT NULL,
                    [SupportsMac] int NOT NULL
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
                @DeletedOn datetime2
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
                ORDER BY g.Id;
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
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0
                ORDER BY g.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                ORDER BY g.Id;
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
                ORDER BY g.Id;
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
                ORDER BY g.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetBySteamGameId = new()
    {
        Table = Table,
        Action = "GetBySteamGameId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetBySteamGameId]
                @SteamGameId int
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.SteamGameId = @SteamGameId AND g.IsDeleted = 0
                ORDER BY g.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetBySteamToolId = new()
    {
        Table = Table,
        Action = "GetBySteamToolId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetBySteamToolId]
                @SteamToolId int
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.SteamToolId = @SteamToolId AND g.IsDeleted = 0
                ORDER BY g.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @FriendlyName NVARCHAR(128),
                @SteamName NVARCHAR(128),
                @SteamGameId int,
                @SteamToolId int,
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
                @PriceDiscount int,
                @MetaCriticScore int,
                @UrlMetaCriticPage NVARCHAR(128),
                @RequirementsPcMinimum NVARCHAR(128),
                @RequirementsPcRecommended NVARCHAR(128),
                @RequirementsMacMinimum NVARCHAR(128),
                @RequirementsMacRecommended NVARCHAR(128),
                @RequirementsLinuxMinimum NVARCHAR(128),
                @RequirementsLinuxRecommended NVARCHAR(128),
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn datetime2,
                @LastModifiedBy UNIQUEIDENTIFIER,
                @LastModifiedOn datetime2,
                @IsDeleted BIT,
                @SupportsWindows int,
                @SupportsLinux int,
                @SupportsMac int
            AS
            begin
                INSERT into dbo.[{Table.TableName}]  (FriendlyName, SteamName, SteamGameId, SteamToolId, DefaultGameProfileId, LatestBuildVersion, UrlBackground, UrlLogo,
                                                      UrlLogoSmall, UrlWebsite, ControllerSupport, DescriptionShort, DescriptionLong, DescriptionAbout, PriceInitial, PriceCurrent,
                                                      PriceDiscount, MetaCriticScore, UrlMetaCriticPage, RequirementsPcMinimum, RequirementsPcRecommended, RequirementsMacMinimum,
                                                      RequirementsMacRecommended, RequirementsLinuxMinimum, RequirementsLinuxRecommended, CreatedBy, CreatedOn, LastModifiedBy,
                                                      LastModifiedOn, IsDeleted, SupportsWindows, SupportsLinux, SupportsMac)
                OUTPUT INSERTED.Id
                VALUES (@FriendlyName, @SteamName, @SteamGameId, @SteamToolId, @DefaultGameProfileId, @LatestBuildVersion, @UrlBackground, @UrlLogo, @UrlLogoSmall, @UrlWebsite,
                        @ControllerSupport, @DescriptionShort, @DescriptionLong, @DescriptionAbout, @PriceInitial, @PriceCurrent, @PriceDiscount, @MetaCriticScore,
                        @UrlMetaCriticPage, @RequirementsPcMinimum, @RequirementsPcRecommended, @RequirementsMacMinimum, @RequirementsMacRecommended, @RequirementsLinuxMinimum,
                        @RequirementsLinuxRecommended, @CreatedBy, @CreatedOn, @LastModifiedBy, @LastModifiedOn, @IsDeleted, @SupportsWindows, @SupportsLinux, @SupportsMac);
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
                WHERE g.IsDeleted = 0 AND g.FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR g.SteamName LIKE '%' + @SearchTerm + '%'
                    OR g.SteamGameId LIKE '%' + @SearchTerm + '%'
                    OR g.SteamToolId LIKE '%' + @SearchTerm + '%'
                    OR g.LatestBuildVersion LIKE '%' + @SearchTerm + '%'
                    OR g.DescriptionShort LIKE '%' + @SearchTerm + '%';
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
                
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0 AND g.FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR g.SteamName LIKE '%' + @SearchTerm + '%'
                    OR g.SteamGameId LIKE '%' + @SearchTerm + '%'
                    OR g.SteamToolId LIKE '%' + @SearchTerm + '%'
                    OR g.LatestBuildVersion LIKE '%' + @SearchTerm + '%'
                    OR g.DescriptionShort LIKE '%' + @SearchTerm + '%'
                ORDER BY g.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                @SteamGameId int = null,
                @SteamToolId int = null,
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
                @PriceDiscount int = null,
                @MetaCriticScore int = null,
                @UrlMetaCriticPage NVARCHAR(128) = null,
                @RequirementsPcMinimum NVARCHAR(128) = null,
                @RequirementsPcRecommended NVARCHAR(128) = null,
                @RequirementsMacMinimum NVARCHAR(128) = null,
                @RequirementsMacRecommended NVARCHAR(128) = null,
                @RequirementsLinuxMinimum NVARCHAR(128) = null,
                @RequirementsLinuxRecommended NVARCHAR(128) = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn datetime2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn datetime2 = null,
                @IsDeleted BIT = null,
                @DeletedOn datetime2 = null,
                @SupportsWindows BIT = null,
                @SupportsLinux BIT = null,
                @SupportsMac BIT = null
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
                    SupportsMac = COALESCE(@SupportsMac, SupportsMac)
                WHERE Id = @Id;
            end"
    };
}