using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class LocalResourcesTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "LocalResources";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(LocalResourcesTableMsSql).GetDbScriptsFromClass();

    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 9,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                    [GameProfileId] UNIQUEIDENTIFIER NOT NULL,
                    [Name] NVARCHAR(128) NOT NULL,
                    [PathWindows] NVARCHAR(128) NOT NULL,
                    [PathLinux] NVARCHAR(128) NOT NULL,
                    [PathMac] NVARCHAR(128) NOT NULL,
                    [Startup] BIT NOT NULL,
                    [StartupPriority] int NOT NULL,
                    [Type] INT NOT NULL,
                    [ContentType] INT NOT NULL,
                    [Args] NVARCHAR(128) NOT NULL,
                    [LoadExisting] BIT NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [CreatedOn] DATETIME2 NOT NULL,
                    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
                    [LastModifiedOn] DATETIME2 NULL
                )
            end"
    };

    public static readonly SqlStoredProcedure Delete = new()
    {
        Table = Table,
        Action = "Delete",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Delete]
                @Id UNIQUEIDENTIFIER
            AS
            begin
                DELETE FROM dbo.[{Table.TableName}]
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
                SELECT l.*
                FROM dbo.[{Table.TableName}] l
                ORDER BY l.Name ASC;
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
                SELECT COUNT(*) OVER() AS TotalCount, l.*
                FROM dbo.[{Table.TableName}] l
                ORDER BY l.Name ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                SELECT TOP 1 l.*
                FROM dbo.[{Table.TableName}] l
                WHERE l.Id = @Id
                ORDER BY l.Id;
            end"
    };

    public static readonly SqlStoredProcedure GetByGameProfileId = new()
    {
        Table = Table,
        Action = "GetByGameProfileId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByGameProfileId]
                @GameProfileId UNIQUEIDENTIFIER
            AS
            begin
                SELECT l.*
                FROM dbo.[{Table.TableName}] l
                WHERE l.GameProfileId = @GameProfileId
                ORDER BY l.Id;
            end"
    };

    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @Id UNIQUEIDENTIFIER,
                @GameProfileId UNIQUEIDENTIFIER,
                @Name NVARCHAR(128),
                @PathWindows NVARCHAR(128),
                @PathLinux NVARCHAR(128),
                @PathMac NVARCHAR(128),
                @Startup BIT,
                @StartupPriority INT,
                @Type INT,
                @ContentType INT,
                @Args NVARCHAR(128),
                @LoadExisting INT,
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn DATETIME2,
                @LastModifiedBy UNIQUEIDENTIFIER,
                @LastModifiedOn DATETIME2
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (Id, GameProfileId, Name, PathWindows, PathLinux, PathMac, Startup, StartupPriority, Type, ContentType, Args, LoadExisting,
                                                     CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn)
                OUTPUT INSERTED.Id
                VALUES (@Id, @GameProfileId, @Name, @PathWindows, @PathLinux, @PathMac, @Startup, @StartupPriority, @Type, @ContentType, @Args, @LoadExisting, @CreatedBy,
                        @CreatedOn, @LastModifiedBy, @LastModifiedOn);
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
                
                SELECT l.*
                FROM dbo.[{Table.TableName}] l
                WHERE l.Id LIKE '%' + @SearchTerm + '%'
                    OR l.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR l.Name LIKE '%' + @SearchTerm + '%'
                    OR l.PathWindows LIKE '%' + @SearchTerm + '%'
                    OR l.PathLinux LIKE '%' + @SearchTerm + '%'
                    OR l.PathMac LIKE '%' + @SearchTerm + '%'
                    OR l.Args LIKE '%' + @SearchTerm + '%';
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
                SELECT COUNT(*) OVER() AS TotalCount, l.*
                FROM dbo.[{Table.TableName}] l
                WHERE l.Id LIKE '%' + @SearchTerm + '%'
                    OR l.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR l.Name LIKE '%' + @SearchTerm + '%'
                    OR l.PathWindows LIKE '%' + @SearchTerm + '%'
                    OR l.PathLinux LIKE '%' + @SearchTerm + '%'
                    OR l.PathMac LIKE '%' + @SearchTerm + '%'
                    OR l.Args LIKE '%' + @SearchTerm + '%'
                ORDER BY l.Name ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };

    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Update]
                @Id UNIQUEIDENTIFIER,
                @GameProfileId UNIQUEIDENTIFIER = null,
                @Name NVARCHAR(128) = null,
                @PathWindows NVARCHAR(128) = null,
                @PathLinux NVARCHAR(128) = null,
                @PathMac NVARCHAR(128) = null,
                @Startup BIT = null,
                @StartupPriority INT = null,
                @Type INT = null,
                @ContentType INT = null,
                @Args NVARCHAR(128) = null,
                @LoadExisting INT = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn DATETIME2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET GameProfileId = COALESCE(@GameProfileId, GameProfileId), Name = COALESCE(@Name, Name),
                    PathWindows = COALESCE(@PathWindows, PathWindows), PathLinux = COALESCE(@PathLinux, PathLinux), PathMac = COALESCE(@PathMac, PathMac),
                    Startup = COALESCE(@Startup, Startup), StartupPriority = COALESCE(@StartupPriority, StartupPriority), Type = COALESCE(@Type, Type),
                    ContentType = COALESCE(@ContentType, ContentType), Args = COALESCE(@Args, Args), LoadExisting = COALESCE(@LoadExisting, LoadExisting),
                    LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy), LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn)
                WHERE Id = @Id;
            end"
    };
}