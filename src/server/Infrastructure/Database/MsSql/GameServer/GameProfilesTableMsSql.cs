using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class GameProfilesTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "GameProfiles";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(GameProfilesTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 1,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [FriendlyName] NVARCHAR(128) NOT NULL,
                    [OwnerId] UNIQUEIDENTIFIER NOT NULL,
                    [GameId] UNIQUEIDENTIFIER NOT NULL,
                    [ServerProcessName] NVARCHAR(128) NOT NULL,
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
                SELECT g.*
                FROM dbo.[{Table.TableName}] g;
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
    
    public static readonly SqlStoredProcedure GetByOwnerId = new()
    {
        Table = Table,
        Action = "GetByOwnerId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByOwnerId]
                @OwnerId UNIQUEIDENTIFIER
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.OwnerId = @OwnerId
                ORDER BY g.Id;
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
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.GameId = @GameId
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
                WHERE g.FriendlyName = @FriendlyName
                ORDER BY g.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByServerProcessName = new()
    {
        Table = Table,
        Action = "GetByServerProcessName",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByServerProcessName]
                @ServerProcessName NVARCHAR(128)
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.ServerProcessName = @ServerProcessName
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
                @OwnerId UNIQUEIDENTIFIER,
                @GameId UNIQUEIDENTIFIER,
                @ServerProcessName NVARCHAR(128),
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn datetime2,
                @LastModifiedBy UNIQUEIDENTIFIER,
                @LastModifiedOn datetime2,
                @IsDeleted BIT,
                @DeletedOn datetime2
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (FriendlyName, OwnerId, GameId, ServerProcessName, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsDeleted, DeletedOn)
                OUTPUT INSERTED.Id
                VALUES (@FriendlyName, @OwnerId, @GameId, @ServerProcessName, @CreatedBy, @CreatedOn, @LastModifiedBy, @LastModifiedOn, @IsDeleted, @DeletedOn);
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
                WHERE g.FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR g.OwnerId LIKE '%' + @SearchTerm + '%'
                    OR g.GameId LIKE '%' + @SearchTerm + '%'
                    OR g.ServerProcessName LIKE '%' + @SearchTerm + '%';
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
                WHERE g.FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR g.OwnerId LIKE '%' + @SearchTerm + '%'
                    OR g.GameId LIKE '%' + @SearchTerm + '%'
                    OR g.ServerProcessName LIKE '%' + @SearchTerm + '%'
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
                @OwnerId UNIQUEIDENTIFIER = null,
                @GameId UNIQUEIDENTIFIER = null,
                @ServerProcessName NVARCHAR(128) = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn datetime2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn datetime2 = null,
                @IsDeleted BIT = null,
                @DeletedOn datetime2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET FriendlyName = COALESCE(@FriendlyName, FriendlyName), OwnerId = COALESCE(@OwnerId, OwnerId), GameId = COALESCE(@GameId, GameId),
                    ServerProcessName = COALESCE(@ServerProcessName, ServerProcessName), CreatedBy = COALESCE(@CreatedBy, CreatedBy), CreatedOn = COALESCE(@CreatedOn, CreatedOn),
                    LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy), LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn),
                    IsDeleted = COALESCE(@IsDeleted, IsDeleted), DeletedOn = COALESCE(@DeletedOn, DeletedOn)
                WHERE Id = @Id;
            end"
    };
}