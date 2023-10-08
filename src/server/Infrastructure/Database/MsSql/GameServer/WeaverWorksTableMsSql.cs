using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class WeaverWorksTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "WeaverWorks";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(WeaverWorksTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 1,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [HostId] UNIQUEIDENTIFIER NOT NULL,
                    [GameServerId] UNIQUEIDENTIFIER NULL,
                    [TargetType] int NOT NULL,
                    [Status] int NOT NULL,
                    [WorkData] NVARCHAR(2048) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [CreatedOn] datetime2 NOT NULL,
                    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
                    [LastModifiedOn] datetime2 NULL
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
                SELECT w.*
                FROM dbo.[{Table.TableName}] w;
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
                SELECT w.*
                FROM dbo.[{Table.TableName}] w
                ORDER BY w.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                SELECT TOP 1 w.*
                FROM dbo.[{Table.TableName}] w
                WHERE w.Id = @Id
                ORDER BY w.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByHostId = new()
    {
        Table = Table,
        Action = "GetByHostId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByHostId]
                @HostId UNIQUEIDENTIFIER
            AS
            begin
                SELECT w.*
                FROM dbo.[{Table.TableName}] w
                WHERE w.HostId = @HostId
                ORDER BY w.Id;
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
                SELECT w.*
                FROM dbo.[{Table.TableName}] w
                WHERE w.TargetType = 1 AND w.GameServerId = @GameServerId
                ORDER BY w.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByTargetType = new()
    {
        Table = Table,
        Action = "GetByTargetType",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByTargetType]
                @TargetType int
            AS
            begin
                SELECT w.*
                FROM dbo.[{Table.TableName}] w
                WHERE w.TargetType = @TargetType
                ORDER BY w.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByStatus = new()
    {
        Table = Table,
        Action = "GetByStatus",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByStatus]
                @Status int
            AS
            begin
                SELECT w.*
                FROM dbo.[{Table.TableName}] w
                WHERE w.Status = @Status
                ORDER BY w.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @HostId UNIQUEIDENTIFIER,
                @GameServerId UNIQUEIDENTIFIER,
                @TargetType int,
                @Status int,
                @WorkData NVARCHAR(2048),
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn datetime2,
                @LastModifiedBy UNIQUEIDENTIFIER,
                @LastModifiedOn datetime2
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (HostId, GameServerId, TargetType, Status, WorkData, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn);
                OUTPUT INSERTED.Id
                VALUES (@HostId, @GameServerId, @TargetType, @Status, @WorkData, @CreatedBy, @CreatedOn, @LastModifiedBy, @LastModifiedOn);
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
                
                SELECT w.*
                FROM dbo.[{Table.TableName}] w
                WHERE w.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR w.Path LIKE '%' + @SearchTerm + '%'
                    OR w.Category LIKE '%' + @SearchTerm + '%'
                    OR w.Key LIKE '%' + @SearchTerm + '%'
                    OR w.Value LIKE '%' + @SearchTerm + '%';
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
                
                SELECT w.*
                FROM dbo.[{Table.TableName}] w
                WHERE w.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR w.Path LIKE '%' + @SearchTerm + '%'
                    OR w.Category LIKE '%' + @SearchTerm + '%'
                    OR w.Key LIKE '%' + @SearchTerm + '%'
                    OR w.Value LIKE '%' + @SearchTerm + '%'
                ORDER BY w.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };
    
    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Update]
                @Id UNIQUEIDENTIFIER,
                @HostId UNIQUEIDENTIFIER = null,
                @GameServerId UNIQUEIDENTIFIER = null,
                @TargetType int = null,
                @Status int = null,
                @WorkData NVARCHAR(2048) = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn datetime2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn datetime2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET Id = COALESCE(@Id, Id), HostId = COALESCE(@HostId, HostId), GameServerId = COALESCE(@GameServerId, GameServerId),
                    TargetType = COALESCE(@TargetType, TargetType), Status = COALESCE(@Status, Status), WorkData = COALESCE(@WorkData, WorkData),
                    CreatedBy = COALESCE(@CreatedBy, CreatedBy), CreatedOn = COALESCE(@CreatedOn, CreatedOn), LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy),
                    LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn)
                WHERE Id = @Id;
            end"
    };
}