using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class PublishersTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "Publishers";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(PublishersTableMsSql).GetDbScriptsFromClass();

    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 10,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                    [GameId] UNIQUEIDENTIFIER NOT NULL,
                    [Name] NVARCHAR(128) NOT NULL
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
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                ORDER BY p.Name ASC;
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
                SELECT COUNT(*) OVER() AS TotalCount, p.*
                FROM dbo.[{Table.TableName}] p
                ORDER BY p.Name ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                SELECT TOP 1 p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.Id = @Id
                ORDER BY p.Id;
            end"
    };

    public static readonly SqlStoredProcedure GetByGameId = new()
    {
        Table = Table,
        Action = "GetByGameId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByGameId]
                @GameId UNIQUEIDENTIFIER
            AS
            begin
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.GameId = @GameId
                ORDER BY p.Id;
            end"
    };

    public static readonly SqlStoredProcedure GetByName = new()
    {
        Table = Table,
        Action = "GetByName",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByName]
                @Name NVARCHAR(128)
            AS
            begin
                SELECT TOP 1 p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.Name = @Name
                ORDER BY p.Id;
            end"
    };

    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @Id UNIQUEIDENTIFIER,
                @GameId UNIQUEIDENTIFIER,
                @Name NVARCHAR(128)
            AS
            begin
                INSERT into dbo.[{Table.TableName}]  (Id, GameId, Name)
                OUTPUT INSERTED.Id
                VALUES (@Id, @GameId, @Name);
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
                
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.Id LIKE '%' + @SearchTerm + '%'
                    OR p.Name LIKE '%' + @SearchTerm + '%';
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
                SELECT COUNT(*) OVER() AS TotalCount, p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.Id LIKE '%' + @SearchTerm + '%'
                    OR p.Name LIKE '%' + @SearchTerm + '%'
                ORDER BY p.Name ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };
}