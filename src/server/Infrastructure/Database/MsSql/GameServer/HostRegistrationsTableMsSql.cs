using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class HostRegistrationsTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "HostRegistrations";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(HostRegistrationsTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 10,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [HostId] UNIQUEIDENTIFIER NOT NULL,
                    [Description] NVARCHAR(2048) NOT NULL,
                    [Active] BIT NOT NULL,
                    [Key] NVARCHAR(256) NOT NULL,
                    [ActivationDate] datetime2 NULL,
                    [ActivationPublicIp] NVARCHAR(128) NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [CreatedOn] datetime2 NOT NULL,
                    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
                    [LastModifiedOn] datetime2 NULL
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
                SELECT h.*
                FROM dbo.[{Table.TableName}] h;
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
                SELECT COUNT(*) OVER() AS TotalCount, h.*
                FROM dbo.[{Table.TableName}] h
                ORDER BY h.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };
    
    public static readonly SqlStoredProcedure GetAllActive = new()
    {
        Table = Table,
        Action = "GetAllActive",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllActive]
            AS
            begin
                SELECT h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.Active = 1;
            end"
    };
    
    public static readonly SqlStoredProcedure GetAllInActive = new()
    {
        Table = Table,
        Action = "GetAllInActive",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllInActive]
            AS
            begin
                SELECT h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.Active = 0;
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
                SELECT TOP 1 h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.Id = @Id
                ORDER BY h.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByHostId = new()
    {
        Table = Table,
        Action = "GetByHostId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByHostId]
                @HostId NVARCHAR(256)
            AS
            begin
                SELECT h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.HostId = @HostId
                ORDER BY h.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByHostIdAndKey = new()
    {
        Table = Table,
        Action = "GetByHostIdAndKey",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByHostIdAndKey]
                @HostId NVARCHAR(256),
                @Key NVARCHAR(256)
            AS
            begin
                SELECT TOP 1 h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.HostId = @HostId AND h.[Key] = @Key AND h.Active = 1
                ORDER BY h.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetActiveByDescription = new()
    {
        Table = Table,
        Action = "GetActiveByDescription",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetActiveByDescription]
                @Description NVARCHAR(256)
            AS
            begin
                SELECT h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.Active = 1 AND h.Description = @Description
                ORDER BY h.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @HostId UNIQUEIDENTIFIER,
                @Description NVARCHAR(2048),
                @Active BIT,
                @Key NVARCHAR(256),
                @ActivationDate datetime2,
                @ActivationPublicIp NVARCHAR(128),
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn datetime2,
                @LastModifiedBy UNIQUEIDENTIFIER,
                @LastModifiedOn datetime2
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (HostId, Description, Active, [Key], ActivationDate, ActivationPublicIp, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn)
                OUTPUT INSERTED.Id
                VALUES (@HostId, @Description, @Active, @Key, @ActivationDate, @ActivationPublicIp, @CreatedBy, @CreatedOn, @LastModifiedBy, @LastModifiedOn);
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
                
                SELECT h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.Id LIKE '%' + @SearchTerm + '%'
                    OR h.HostId LIKE '%' + @SearchTerm + '%'
                    OR h.[Key] LIKE '%' + @SearchTerm + '%'
                    OR h.Description LIKE '%' + @SearchTerm + '%'
                    OR h.ActivationPublicIp LIKE '%' + @SearchTerm + '%';
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
                SELECT COUNT(*) OVER() AS TotalCount, h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.Id LIKE '%' + @SearchTerm + '%'
                    OR h.HostId LIKE '%' + @SearchTerm + '%'
                    OR h.[Key] LIKE '%' + @SearchTerm + '%'
                    OR h.Description LIKE '%' + @SearchTerm + '%'
                    OR h.ActivationPublicIp LIKE '%' + @SearchTerm + '%'
                ORDER BY h.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                @Description NVARCHAR(2048) = null,
                @Active BIT = null,
                @Key NVARCHAR(256) = null,
                @ActivationDate datetime2 = null,
                @ActivationPublicIp NVARCHAR(128) = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn datetime2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn datetime2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET HostId = COALESCE(@HostId, HostId), Description = COALESCE(@Description, Description), Active = COALESCE(@Active, Active), [Key] = COALESCE(@Key, [Key]),
                    ActivationDate = COALESCE(@ActivationDate, ActivationDate), ActivationPublicIp = COALESCE(@ActivationPublicIp, ActivationPublicIp),
                    CreatedBy = COALESCE(@CreatedBy, CreatedBy), CreatedOn = COALESCE(@CreatedOn, CreatedOn), LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy),
                    LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn)
                WHERE Id = @Id;
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
                WHERE Active = 0
                    AND CreatedOn < @OlderThan;
            end"
    };
}