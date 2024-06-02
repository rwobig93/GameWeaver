using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class GameUpdatesTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "GameUpdates";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(GameUpdatesTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 10,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [GameId] UNIQUEIDENTIFIER NOT NULL,
                    [SupportsWindows] int NOT NULL,
                    [SupportsLinux] int NOT NULL,
                    [SupportsMac] int NOT NULL,
                    [BuildVersion] NVARCHAR(128) NOT NULL,
                    [BuildVersionReleased] datetime2 NOT NULL
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
    
    public static readonly SqlStoredProcedure DeleteForGameId = new()
    {
        Table = Table,
        Action = "DeleteForGameId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_DeleteForGameId]
                @Id UNIQUEIDENTIFIER
            AS
            begin
                DELETE FROM dbo.[{Table.TableName}]
                WHERE GameId = @Id;
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
                ORDER BY g.BuildVersionReleased DESC;
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
                ORDER BY g.BuildVersionReleased DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                WHERE g.Id = @Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByGameId = new()
    {
        Table = Table,
        Action = "GetByGameId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByGameId]
                @Id UNIQUEIDENTIFIER
            AS
            begin
                SELECT TOP 1 g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.GameId = @Id;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @GameId UNIQUEIDENTIFIER,
                @SupportsWindows int,
                @SupportsLinux int,
                @SupportsMac int,
                @BuildVersion NVARCHAR(128),
                @BuildVersionReleased datetime2
            AS
            begin
                INSERT into dbo.[{Table.TableName}]  (GameId, SupportsWindows, SupportsLinux, SupportsMac, BuildVersion, BuildVersionReleased)
                OUTPUT INSERTED.Id
                VALUES (@GameId, @SupportsWindows, @SupportsLinux, @SupportsMac, @BuildVersion, @BuildVersionReleased);
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
                WHERE g.GameId LIKE '%' + @SearchTerm + '%'
                    OR g.BuildVersion LIKE '%' + @SearchTerm + '%';
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
                WHERE g.GameId LIKE '%' + @SearchTerm + '%'
                    OR g.BuildVersion LIKE '%' + @SearchTerm + '%'
                ORDER BY g.BuildVersionReleased DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };
}