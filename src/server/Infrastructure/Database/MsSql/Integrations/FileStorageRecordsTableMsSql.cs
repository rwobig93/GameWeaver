using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.Integrations;

public class FileStorageRecordsTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "FileStorageRecords";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(FileStorageRecordsTableMsSql).GetDbScriptsFromClass();

    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 4,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                    [Format] INT NOT NULL,
                    [LinkedType] INT NOT NULL,
                    [LinkedId] UNIQUEIDENTIFIER NOT NULL,
                    [FriendlyName] NVARCHAR(100) NOT NULL,
                    [Filename] NVARCHAR(100) NOT NULL,
                    [Description] NVARCHAR(2048) NOT NULL,
                    [HashSha256] NVARCHAR(2048) NOT NULL,
                    [Version] NVARCHAR(128) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [CreatedOn] DATETIME2 NOT NULL,
                    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
                    [LastModifiedOn] DATETIME2 NULL,
                    [IsDeleted] BIT NOT NULL,
                    [DeletedOn] DATETIME2 NULL
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
                WHERE IsDeleted = 0
                ORDER BY CreatedOn DESC;
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
                WHERE IsDeleted = 0
                ORDER BY CreatedOn DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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

    public static readonly SqlStoredProcedure GetByFormat = new()
    {
        Table = Table,
        Action = "GetByFormat",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByFormat]
                @Format INT
            AS
            begin
                SELECT r.*
                FROM dbo.[{Table.TableName}] r
                WHERE r.Format = @Format AND r.IsDeleted = 0
                ORDER BY CreatedOn DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetByLinkedId = new()
    {
        Table = Table,
        Action = "GetByLinkedId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByLinkedId]
                @LinkedId UNIQUEIDENTIFIER
            AS
            begin
                SELECT r.*
                FROM dbo.[{Table.TableName}] r
                WHERE r.LinkedId = @LinkedId AND r.IsDeleted = 0
                ORDER BY CreatedOn DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetByLinkedType = new()
    {
        Table = Table,
        Action = "GetByLinkedType",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByLinkedType]
                @LinkedType INT
            AS
            begin
                SELECT r.*
                FROM dbo.[{Table.TableName}] r
                WHERE r.LinkedType = @LinkedType AND r.IsDeleted = 0
                ORDER BY CreatedOn DESC;
            end"
    };

    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @Id UNIQUEIDENTIFIER,
                @Format INT,
                @LinkedType INT,
                @LinkedId UNIQUEIDENTIFIER,
                @FriendlyName NVARCHAR(100),
                @Filename NVARCHAR(100),
                @Description NVARCHAR(2048),
                @HashSha256 NVARCHAR(2048),
                @Version NVARCHAR(128),
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn DATETIME2,
                @LastModifiedBy UNIQUEIDENTIFIER,
                @LastModifiedOn DATETIME2,
                @IsDeleted BIT,
                @DeletedOn DATETIME2
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (Id, Format, LinkedType, LinkedId, FriendlyName, Filename, Description, HashSha256, Version, CreatedBy, CreatedOn, LastModifiedBy,
                                                     LastModifiedOn, IsDeleted, DeletedOn)
                OUTPUT INSERTED.Id
                VALUES (@Id, @Format, @LinkedType, @LinkedId, @FriendlyName, @Filename, @Description, @HashSha256, @Version, @CreatedBy, @CreatedOn, @LastModifiedBy, @LastModifiedOn,
                        @IsDeleted, @DeletedOn);
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
                WHERE IsDeleted = 0 AND Id LIKE '%' + @SearchTerm + '%'
                    OR LinkedId LIKE '%' + @SearchTerm + '%'
                    OR FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR Filename LIKE '%' + @SearchTerm + '%'
                    OR Description LIKE '%' + @SearchTerm + '%'
                    OR HashSha256 LIKE '%' + @SearchTerm + '%'
                    OR Version LIKE '%' + @SearchTerm + '%'
                ORDER BY CreatedOn DESC;
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
                WHERE IsDeleted = 0 AND Id LIKE '%' + @SearchTerm + '%'
                    OR LinkedId LIKE '%' + @SearchTerm + '%'
                    OR FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR Filename LIKE '%' + @SearchTerm + '%'
                    OR Description LIKE '%' + @SearchTerm + '%'
                    OR HashSha256 LIKE '%' + @SearchTerm + '%'
                    OR Version LIKE '%' + @SearchTerm + '%'
                ORDER BY CreatedOn DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };

    public static readonly SqlStoredProcedure Delete = new()
    {
        Table = Table,
        Action = "Delete",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Delete]
                @Id UNIQUEIDENTIFIER,
                @DeletedBy UNIQUEIDENTIFIER,
                @DeletedOn datetime2
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET IsDeleted = 1, DeletedOn = @DeletedOn, LastModifiedBy = @DeletedBy
                WHERE Id = @Id;
            end"
    };

    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Update]
                @Id UNIQUEIDENTIFIER,
                @Format INT = null,
                @LinkedType INT = null,
                @LinkedId UNIQUEIDENTIFIER = null,
                @FriendlyName NVARCHAR(100) = null,
                @Filename NVARCHAR(100) = null,
                @Description NVARCHAR(2048) = null,
                @HashSha256 NVARCHAR(2048) = null,
                @Version NVARCHAR(128) = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn DATETIME2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn DATETIME2 = null,
                @IsDeleted BIT = null,
                @DeletedOn DATETIME2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET Format = COALESCE(@Format, Format), LinkedType = COALESCE(@LinkedType, LinkedType), LinkedId = COALESCE(@LinkedId, LinkedId),
                    FriendlyName = COALESCE(@FriendlyName, FriendlyName), Filename = COALESCE(@Filename, Filename), Description = COALESCE(@Description, Description),
                    HashSha256 = COALESCE(@HashSha256, HashSha256), Version = COALESCE(@Version, Version), CreatedBy = COALESCE(@CreatedBy, CreatedBy),
                    CreatedOn = COALESCE(@CreatedOn, CreatedOn), LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy),
                    LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn), IsDeleted = COALESCE(@IsDeleted, IsDeleted), DeletedOn = COALESCE(@DeletedOn, DeletedOn)
                WHERE Id = @Id;
            end"
    };
}