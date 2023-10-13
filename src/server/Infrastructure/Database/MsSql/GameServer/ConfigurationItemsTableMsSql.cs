using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class ConfigurationItemsTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "ConfigurationItems";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(ConfigurationItemsTableMsSql).GetDbScriptsFromClass();
    
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
                    [Path] NVARCHAR(128) NOT NULL,
                    [Category] NVARCHAR(128) NOT NULL,
                    [Key] NVARCHAR(128) NOT NULL,
                    [Value] NVARCHAR(128) NOT NULL
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
                SELECT c.*
                FROM dbo.[{Table.TableName}] c;
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
                SELECT c.*
                FROM dbo.[{Table.TableName}] c
                ORDER BY c.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                SELECT TOP 1 c.*
                FROM dbo.[{Table.TableName}] c
                WHERE c.Id = @Id
                ORDER BY c.Id;
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
                SELECT c.*
                FROM dbo.[{Table.TableName}] c
                WHERE c.GameProfileId = @GameProfileId
                ORDER BY c.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @GameProfileId UNIQUEIDENTIFIER,
                @Path NVARCHAR(128),
                @Category NVARCHAR(128),
                @Key NVARCHAR(128),
                @Value NVARCHAR(128)
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (GameProfileId, Path, Category, Key, Value);
                OUTPUT INSERTED.Id
                VALUES (@GameProfileId, @Path, @Category, @Key, @Value);
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
                
                SELECT c.*
                FROM dbo.[{Table.TableName}] c
                WHERE c.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR c.Path LIKE '%' + @SearchTerm + '%'
                    OR c.Category LIKE '%' + @SearchTerm + '%'
                    OR c.Key LIKE '%' + @SearchTerm + '%'
                    OR c.Value LIKE '%' + @SearchTerm + '%';
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
                
                SELECT c.*
                FROM dbo.[{Table.TableName}] c
                WHERE c.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR c.Path LIKE '%' + @SearchTerm + '%'
                    OR c.Category LIKE '%' + @SearchTerm + '%'
                    OR c.Key LIKE '%' + @SearchTerm + '%'
                    OR c.Value LIKE '%' + @SearchTerm + '%'
                ORDER BY c.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                @Path NVARCHAR(128) = null,
                @Category NVARCHAR(128) = null,
                @Key NVARCHAR(128) = null,
                @Value NVARCHAR(128) = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET GameProfileId = COALESCE(@GameProfileId, GameProfileId), Path = COALESCE(@Path, Path), Category = COALESCE(@Category, Category), Key = COALESCE(@Key, Key),
                    Value = COALESCE(@Value, Value)
                WHERE Id = @Id;
            end"
    };
}