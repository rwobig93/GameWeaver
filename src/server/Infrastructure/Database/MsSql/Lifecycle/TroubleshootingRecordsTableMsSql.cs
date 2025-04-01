using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;
using Infrastructure.Database.MsSql.Identity;

namespace Infrastructure.Database.MsSql.Lifecycle;

public class TroubleshootingRecordsTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "TroubleshootingRecords";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(TroubleshootingRecordsTableMsSql).GetDbScriptsFromClass();

    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 4,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                    [EntityType] INT NOT NULL,
                    [RecordId] UNIQUEIDENTIFIER NOT NULL,
                    [ChangedBy] UNIQUEIDENTIFIER NOT NULL,
                    [Timestamp] DATETIME2 NOT NULL,
                    [Message] NVARCHAR(1024) NOT NULL,
                    [Detail] NVARCHAR(MAX) NULL
                )
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
                SELECT *
                FROM dbo.[{Table.TableName}]
                ORDER BY Timestamp DESC;
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
                SELECT COUNT(*) OVER() AS TotalCount, *
                FROM dbo.[{Table.TableName}]
                ORDER BY Timestamp DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                SELECT TOP 1 *
                FROM dbo.[{Table.TableName}]
                WHERE Id = @Id
                ORDER BY Id;
            end"
    };

    public static readonly SqlStoredProcedure GetByChangedBy = new()
    {
        Table = Table,
        Action = "GetByChangedBy",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByChangedBy]
                @UserId UNIQUEIDENTIFIER
            AS
            begin
                SELECT a.*, u.Id as ChangedBy, u.Username as ChangedByUsername
                FROM dbo.[{Table.TableName}] a
                JOIN {AppUsersTableMsSql.Table.TableName} u ON a.ChangedBy = u.Id
                WHERE a.ChangedBy = @UserId
                ORDER BY Timestamp DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetByEntityType = new()
    {
        Table = Table,
        Action = "GetByEntityType",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByEntityType]
                @EntityType INT
            AS
            begin
                SELECT a.*
                FROM dbo.[{Table.TableName}] a
                WHERE a.EntityType = @EntityType
                ORDER BY Timestamp DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetByRecordId = new()
    {
        Table = Table,
        Action = "GetByRecordId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByRecordId]
                @RecordId UNIQUEIDENTIFIER
            AS
            begin
                SELECT a.*, u.Id as ChangedBy, u.Username as ChangedByUsername
                FROM dbo.[{Table.TableName}] a
                JOIN {AppUsersTableMsSql.Table.TableName} u ON a.ChangedBy = u.Id
                WHERE a.RecordId = @RecordId
                ORDER BY Timestamp DESC;
            end"
    };

    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @Id UNIQUEIDENTIFIER,
                @EntityType INT,
                @RecordId UNIQUEIDENTIFIER,
                @ChangedBy UNIQUEIDENTIFIER,
                @Timestamp DATETIME2,
                @Message NVARCHAR(1024),
                @Detail NVARCHAR(MAX)
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (Id, EntityType, RecordId, ChangedBy, Timestamp, Message, Detail)
                OUTPUT INSERTED.Id
                VALUES (@Id, @EntityType, @RecordId, @ChangedBy, @Timestamp, @Message, @Detail);
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
                set nocount on;
                
                SELECT *
                FROM dbo.[{Table.TableName}]
                WHERE Id LIKE '%' + @SearchTerm + '%'
                    OR RecordId LIKE '%' + @SearchTerm + '%'
                    OR Message LIKE '%' + @SearchTerm + '%'
                    OR Detail LIKE '%' + @SearchTerm + '%'
                ORDER BY Timestamp DESC;
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
                SELECT COUNT(*) OVER() AS TotalCount, *
                FROM dbo.[{Table.TableName}]
                WHERE Id LIKE '%' + @SearchTerm + '%'
                    OR RecordId LIKE '%' + @SearchTerm + '%'
                    OR Message LIKE '%' + @SearchTerm + '%'
                    OR Detail LIKE '%' + @SearchTerm + '%'
                ORDER BY Timestamp DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };
    
    public static readonly SqlStoredProcedure DeleteOlderThan = new()
    {
        Table = Table,
        Action = "DeleteOlderThan",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_DeleteOlderThan]
                @OlderThan DATETIME2
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
                WHERE Timestamp < @OlderThan;
            end"
    };
}