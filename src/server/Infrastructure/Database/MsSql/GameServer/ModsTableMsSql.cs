using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class ModsTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "Mods";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(ModsTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 1,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [GameId] UNIQUEIDENTIFIER NOT NULL,
                    [SteamGameId] int NOT NULL,
                    [SteamToolId] int NOT NULL,
                    [SteamId] NVARCHAR(128) NOT NULL,
                    [FriendlyName] NVARCHAR(128) NOT NULL,
                    [CurrentHash] NVARCHAR(128) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [CreatedOn] datetime2 NOT NULL,
                    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
                    [LastModifiedOn] datetime2 NULL,
                    [IsDeleted] BIT NOT NULL,
                    [DeletedOn] datetime2 NULL
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
                UPDATE dbo.[{Table.TableName}]
                SET IsDeleted = 1, DeletedOn = @DeletedOn
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
                SELECT m.*
                FROM dbo.[{Table.TableName}] m
                WHERE m.IsDeleted = 0;
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
                SELECT m.*
                FROM dbo.[{Table.TableName}] m
                WHERE m.IsDeleted = 0
                ORDER BY m.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                SELECT TOP 1 m.*
                FROM dbo.[{Table.TableName}] m
                WHERE m.Id = @Id
                ORDER BY m.Id;
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
                SELECT m.*
                FROM dbo.[{Table.TableName}] m
                WHERE m.GameId = @GameId AND m.IsDeleted = 0
                ORDER BY m.Id;
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
                SELECT m.*
                FROM dbo.[{Table.TableName}] m
                WHERE m.SteamGameId = @SteamGameId AND m.IsDeleted = 0
                ORDER BY m.Id;
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
                SELECT m.*
                FROM dbo.[{Table.TableName}] m
                WHERE m.SteamToolId = @SteamToolId AND m.IsDeleted = 0
                ORDER BY m.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetBySteamId = new()
    {
        Table = Table,
        Action = "GetBySteamId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetBySteamId]
                @SteamId NVARCHAR(128)
            AS
            begin
                SELECT m.*
                FROM dbo.[{Table.TableName}] m
                WHERE m.SteamId = @SteamId AND m.IsDeleted = 0
                ORDER BY m.Id;
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
                SELECT m.*
                FROM dbo.[{Table.TableName}] m
                WHERE m.FriendlyName = @FriendlyName AND m.IsDeleted = 0
                ORDER BY m.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByCurrentHash = new()
    {
        Table = Table,
        Action = "GetByCurrentHash",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByCurrentHash]
                @CurrentHash NVARCHAR(128)
            AS
            begin
                SELECT m.*
                FROM dbo.[{Table.TableName}] m
                WHERE m.CurrentHash = @CurrentHash
                ORDER BY m.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @GameId UNIQUEIDENTIFIER,
                @SteamGameId int,
                @SteamToolId int,
                @SteamId NVARCHAR(128),
                @FriendlyName NVARCHAR(128),
                @CurrentHash NVARCHAR(128),
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn datetime2,
                @LastModifiedBy UNIQUEIDENTIFIER,
                @LastModifiedOn datetime2,
                @IsDeleted BIT,
                @DeletedOn datetime2
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (GameId, SteamGameId, SteamToolId, SteamId, FriendlyName, CurrentHash, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn,
                                                     IsDeleted, DeletedOn)
                OUTPUT INSERTED.Id
                VALUES (@GameId, @SteamGameId, @SteamToolId, @SteamId, @FriendlyName, @CurrentHash, @CreatedBy, @CreatedOn, @LastModifiedBy, @LastModifiedOn, @IsDeleted,
                        @DeletedOn);
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
                
                SELECT m.*
                FROM dbo.[{Table.TableName}] m
                WHERE m.IsDeleted = 0 AND m.GameId LIKE '%' + @SearchTerm + '%'
                    OR m.SteamGameId LIKE '%' + @SearchTerm + '%'
                    OR m.SteamToolId LIKE '%' + @SearchTerm + '%'
                    OR m.SteamId LIKE '%' + @SearchTerm + '%'
                    OR m.FriendlyName LIKE '%' + @SearchTerm + '%';
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
                
                SELECT m.*
                FROM dbo.[{Table.TableName}] m
                WHERE m.IsDeleted = 0 AND m.GameId LIKE '%' + @SearchTerm + '%'
                    OR m.SteamGameId LIKE '%' + @SearchTerm + '%'
                    OR m.SteamToolId LIKE '%' + @SearchTerm + '%'
                    OR m.SteamId LIKE '%' + @SearchTerm + '%'
                    OR m.FriendlyName LIKE '%' + @SearchTerm + '%'
                ORDER BY m.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };
    
    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Update]
                @Id UNIQUEIDENTIFIER,
                @GameId UNIQUEIDENTIFIER = null,
                @SteamGameId int = null,
                @SteamToolId int = null,
                @SteamId NVARCHAR(128) = null,
                @FriendlyName NVARCHAR(128) = null,
                @CurrentHash NVARCHAR(128) = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn datetime2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn datetime2 = null,
                @IsDeleted BIT = null,
                @DeletedOn datetime2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET GameId = COALESCE(@GameId, GameId), SteamGameId = COALESCE(@SteamGameId, SteamGameId), SteamToolId = COALESCE(@SteamToolId, SteamToolId),
                    SteamId = COALESCE(@SteamId, SteamId), FriendlyName = COALESCE(@FriendlyName, FriendlyName), CurrentHash = COALESCE(@CurrentHash, CurrentHash),
                    CreatedBy = COALESCE(@CreatedBy, CreatedBy), CreatedOn = COALESCE(@CreatedOn, CreatedOn), LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy),
                    LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn), IsDeleted = COALESCE(@IsDeleted, IsDeleted), DeletedOn = COALESCE(@DeletedOn, DeletedOn)
                WHERE Id = @Id;
            end"
    };
}