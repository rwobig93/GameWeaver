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
        EnforcementOrder = 1,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [GameProfileId] UNIQUEIDENTIFIER NOT NULL,
                    [GameServerId] UNIQUEIDENTIFIER NOT NULL,
                    [Name] NVARCHAR(128) NOT NULL,
                    [Path] NVARCHAR(128) NOT NULL,
                    [Startup] BIT NOT NULL,
                    [StartupPriority] int NOT NULL,
                    [Type] int NOT NULL,
                    [Extension] NVARCHAR(128) NOT NULL,
                    [Args] NVARCHAR(128) NOT NULL
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
                @DeletedOn datetime2
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
                FROM dbo.[{Table.TableName}] l;
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
                SELECT l.*
                FROM dbo.[{Table.TableName}] l
                ORDER BY l.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
    
    public static readonly SqlStoredProcedure GetByGameServerId = new()
    {
        Table = Table,
        Action = "GetByGameServerId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByGameServerId]
                @GameServerId UNIQUEIDENTIFIER
            AS
            begin
                SELECT l.*
                FROM dbo.[{Table.TableName}] l
                WHERE l.GameServerId = @GameServerId
                ORDER BY l.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @GameProfileId UNIQUEIDENTIFIER,
                @GameServerId UNIQUEIDENTIFIER,
                @Name NVARCHAR(128),
                @Path NVARCHAR(128),
                @Startup BIT,
                @StartupPriority int,
                @Type int,
                @Extension NVARCHAR(128),
                @Args NVARCHAR(128)
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (GameProfileId, GameServerId, Name, Path, Startup, StartupPriority, Type, Extension, Args);
                OUTPUT INSERTED.Id
                VALUES (@GameProfileId, @GameServerId, @Name, @Path, @Startup, @StartupPriority, @Type, @Extension, @Args);
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
                WHERE l.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR l.GameServerId LIKE '%' + @SearchTerm + '%'
                    OR l.Name LIKE '%' + @SearchTerm + '%'
                    OR l.Path LIKE '%' + @SearchTerm + '%'
                    OR l.Extension LIKE '%' + @SearchTerm + '%'
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
                SET nocount on;
                
                SELECT l.*
                FROM dbo.[{Table.TableName}] l
                WHERE l.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR l.GameServerId LIKE '%' + @SearchTerm + '%'
                    OR l.Name LIKE '%' + @SearchTerm + '%'
                    OR l.Path LIKE '%' + @SearchTerm + '%'
                    OR l.Extension LIKE '%' + @SearchTerm + '%'
                    OR l.Args LIKE '%' + @SearchTerm + '%'
                ORDER BY l.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                @GameServerId UNIQUEIDENTIFIER = null,
                @Name NVARCHAR(128) = null,
                @Path NVARCHAR(128) = null,
                @Startup BIT = null,
                @StartupPriority int = null,
                @Type int = null,
                @Extension NVARCHAR(128) = null,
                @Args NVARCHAR(128) = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET GameProfileId = COALESECE(@GameProfileId, GameProfileId), GameServerId = COALESECE(@GameServerId, GameServerId), Name = COALESECE(@Name, Name),
                    Path = COALESECE(@Path, Path), Startup = COALESECE(@Startup, Startup), StartupPriority = COALESECE(@StartupPriority, StartupPriority),
                    Type = COALESECE(@Type, Type), Extension = COALESECE(@Extension, Extension), Args = COALESECE(@Args, Args)
                WHERE Id = @Id;
            end"
    };
}