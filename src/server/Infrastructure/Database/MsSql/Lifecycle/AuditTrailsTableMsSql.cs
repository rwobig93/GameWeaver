using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;
using Infrastructure.Database.MsSql.Identity;

namespace Infrastructure.Database.MsSql.Lifecycle;

public class AuditTrailsTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "AuditTrails";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(AuditTrailsTableMsSql).GetDbScriptsFromClass();

    public static readonly SqlTable Table = new()
    {
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [TableName] NVARCHAR(100) NOT NULL,
                    [RecordId] UNIQUEIDENTIFIER NOT NULL,
                    [ChangedBy] UNIQUEIDENTIFIER NOT NULL,
                    [Timestamp] DATETIME2 NOT NULL,
                    [Action] INT NOT NULL,
                    [Before] NVARCHAR(MAX) NOT NULL,
                    [After] NVARCHAR(MAX) NOT NULL
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
    
    public static readonly SqlStoredProcedure GetAllWithUsers = new()
    {
        Table = Table,
        Action = "GetAllWithUsers",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllWithUsers]
            AS
            begin
                SELECT a.*, u.Id as ChangedBy, u.Username as ChangedByUsername
                FROM dbo.[{Table.TableName}] a
                JOIN {AppUsersTableMsSql.Table.TableName} u ON a.ChangedBy = u.Id
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
                SELECT *
                FROM dbo.[{Table.TableName}]
                ORDER BY Timestamp DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };
    
    public static readonly SqlStoredProcedure GetAllPaginatedWithUsers = new()
    {
        Table = Table,
        Action = "GetAllPaginatedWithUsers",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllPaginatedWithUsers]
                @Offset INT,
                @PageSize INT
            AS
            begin
                SELECT a.*, u.Id as ChangedBy, u.Username as ChangedByUsername
                FROM dbo.[{Table.TableName}] a
                JOIN {AppUsersTableMsSql.Table.TableName} u ON a.ChangedBy = u.Id
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

    public static readonly SqlStoredProcedure GetByIdWithUser = new()
    {
        Table = Table,
        Action = "GetByIdWithUser",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByIdWithUser]
                @Id UNIQUEIDENTIFIER
            AS
            begin
                SELECT TOP 1 a.*, u.Id as ChangedBy, u.Username as ChangedByUsername
                FROM dbo.[{Table.TableName}] a
                JOIN {AppUsersTableMsSql.Table.TableName} u ON a.ChangedBy = u.Id
                WHERE a.Id = @Id
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
                @TableName NVARCHAR(100),
                @RecordId UNIQUEIDENTIFIER,
                @ChangedBy UNIQUEIDENTIFIER,
                @Timestamp DATETIME2,
                @Action INT,
                @Before NVARCHAR(MAX),
                @After NVARCHAR(MAX)
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (TableName, RecordId, ChangedBy, Timestamp, Action, Before, After)
                OUTPUT INSERTED.Id
                VALUES (@TableName, @RecordId, @ChangedBy, @Timestamp, @Action, @Before, @After);
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
                WHERE TableName LIKE '%' + @SearchTerm + '%'
                    OR RecordId LIKE '%' + @SearchTerm + '%'
                    OR Action LIKE '%' + @SearchTerm + '%'
                    OR Before LIKE '%' + @SearchTerm + '%'
                    OR After LIKE '%' + @SearchTerm + '%'
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
                set nocount on;
                
                SELECT *
                FROM dbo.[{Table.TableName}]
                WHERE TableName LIKE '%' + @SearchTerm + '%'
                    OR RecordId LIKE '%' + @SearchTerm + '%'
                    OR Action LIKE '%' + @SearchTerm + '%'
                    OR Before LIKE '%' + @SearchTerm + '%'
                    OR After LIKE '%' + @SearchTerm + '%'
                ORDER BY Timestamp DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };

    public static readonly SqlStoredProcedure SearchWithUser = new()
    {
        Table = Table,
        Action = "SearchWithUser",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_SearchWithUser]
                @SearchTerm NVARCHAR(256)
            AS
            begin
                set nocount on;
                
                SELECT a.*, u.Id as ChangedBy, u.Username as ChangedByUsername
                FROM dbo.[{Table.TableName}] a
                JOIN {AppUsersTableMsSql.Table.TableName} u ON a.ChangedBy = u.Id
                WHERE TableName LIKE '%' + @SearchTerm + '%'
                    OR RecordId LIKE '%' + @SearchTerm + '%'
                    OR Action LIKE '%' + @SearchTerm + '%'
                    OR Before LIKE '%' + @SearchTerm + '%'
                    OR After LIKE '%' + @SearchTerm + '%'
                ORDER BY Timestamp DESC;
            end"
    };

    public static readonly SqlStoredProcedure SearchPaginatedWithUser = new()
    {
        Table = Table,
        Action = "SearchPaginatedWithUser",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_SearchPaginatedWithUser]
                @SearchTerm NVARCHAR(256),
                @Offset INT,
                @PageSize INT
            AS
            begin
                set nocount on;
                
                SELECT a.*, u.Id as ChangedBy, u.Username as ChangedByUsername
                FROM dbo.[{Table.TableName}] a
                JOIN {AppUsersTableMsSql.Table.TableName} u ON a.ChangedBy = u.Id
                WHERE TableName LIKE '%' + @SearchTerm + '%'
                    OR RecordId LIKE '%' + @SearchTerm + '%'
                    OR Action LIKE '%' + @SearchTerm + '%'
                    OR Before LIKE '%' + @SearchTerm + '%'
                    OR After LIKE '%' + @SearchTerm + '%'
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