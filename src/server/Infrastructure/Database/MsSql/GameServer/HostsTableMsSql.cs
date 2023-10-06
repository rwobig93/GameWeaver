using Application.Database;
using Application.Database.Providers;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class HostsTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "Hosts";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(HostsTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 1,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [OwnerId] UNIQUEIDENTIFIER NOT NULL,
                    [PasswordHash] NVARCHAR(256) NOT NULL,
                    [PasswordSalt] NVARCHAR(256) NOT NULL,
                    [Hostname] NVARCHAR(256) NULL,
                    [FriendlyName] NVARCHAR(256) NULL,
                    [PrivateIp] NVARCHAR(128) NULL,
                    [PublicIp] NVARCHAR(128) NULL,
                    [CurrentState] INT NOT NULL,
                    [Os] INT NOT NULL,
                    [AllowedPorts] NVARCHAR(2048) NULL,
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
                @Id UNIQUEIDENTIFIER
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
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
                SELECT h.*
                FROM dbo.[{Table.TableName}] h
                ORDER BY h.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
    
    public static readonly SqlStoredProcedure GetByHostname = new()
    {
        Table = Table,
        Action = "GetByHostname",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByHostname]
                @Hostname NVARCHAR(256)
            AS
            begin
                SELECT TOP 1 h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.Hostname = @Hostname
                ORDER BY h.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @OwnerId UNIQUEIDENTIFIER,
                @PasswordHash NVARCHAR(256),
                @PasswordSalt NVARCHAR(256),
                @Hostname NVARCHAR(256),
                @FriendlyName NVARCHAR(256),
                @PrivateIp NVARCHAR(128),
                @PublicIp NVARCHAR(128),
                @CurrentState INT,
                @Os INT,
                @AllowedPorts NVARCHAR(2048),
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn datetime2,
                @LastModifiedBy UNIQUEIDENTIFIER,
                @LastModifiedOn datetime2,
                @IsDeleted BIT,
                @DeletedOn datetime2
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (OwnerId, PasswordHash, PasswordSalt, Hostname, FriendlyName, PrivateIp, PublicIp, CurrentState, Os, AllowedPorts, CreatedBy,
                                                     CreatedOn, LastModifiedBy, LastModifiedOn, IsDeleted, DeletedOn)
                OUTPUT INSERTED.Id
                VALUES (@OwnerId, @PasswordHash, @PasswordSalt, @Hostname, @FriendlyName, @PrivateIp, @PublicIp, @CurrentState, @Os, @AllowedPorts, @CreatedBy, @CreatedOn, 
                        @LastModifiedBy, @LastModifiedOn, @IsDeleted, @DeletedOn);
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
                WHERE h.Hostname LIKE '%' + @SearchTerm + '%'
                    OR h.FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR h.PrivateIp LIKE '%' + @SearchTerm + '%'
                    OR h.PublicIp LIKE '%' + @SearchTerm + '%';
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
                
                SELECT h.*
                FROM dbo.[{Table.TableName}] h
                WHERE h.Hostname LIKE '%' + @SearchTerm + '%'
                    OR h.FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR h.PrivateIp LIKE '%' + @SearchTerm + '%'
                    OR h.PublicIp LIKE '%' + @SearchTerm + '%'
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
                @OwnerId UNIQUEIDENTIFIER = null,
                @PasswordHash NVARCHAR(256) = null,
                @PasswordSalt NVARCHAR(256) = null,
                @Hostname NVARCHAR(256) = null,
                @FriendlyName NVARCHAR(256) = null,
                @PrivateIp NVARCHAR(128) = null,
                @PublicIp NVARCHAR(128) = null,
                @CurrentState INT = null,
                @Os INT = null,
                @AllowedPorts NVARCHAR(2048) = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn datetime2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn datetime2 = null,
                @IsDeleted BIT = null,
                @DeletedOn datetime2 = null,
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET OwnerId = COALESCE(@OwnerId, OwnerId), PasswordHash = COALESCE(@PasswordHash, PasswordHash), PasswordSalt = COALESCE(@PasswordSalt, PasswordSalt),
                    Hostname = COALESCE(@Hostname, Hostname), FriendlyName = COALESCE(@FriendlyName, FriendlyName), PrivateIp = COALESCE(@PrivateIp, PrivateIp),
                    PublicIp = COALESCE(@PublicIp, PublicIp), CurrentState = COALESCE(@CurrentState, CurrentState), Os = COALESCE(@Os, Os),
                    AllowedPorts = COALESCE(@AllowedPorts, AllowedPorts), CreatedBy = COALESCE(@CreatedBy, CreatedBy), CreatedOn = COALESCE(@CreatedOn, CreatedOn),
                    LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy), LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn),
                    IsDeleted = COALESCE(@IsDeleted, IsDeleted), DeletedOn = COALESCE(@DeletedOn, DeletedOn)
                WHERE Id = @Id;
            end"
    };
}