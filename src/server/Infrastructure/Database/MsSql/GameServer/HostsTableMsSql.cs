using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class HostsTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "Hosts";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(HostsTableMsSql).GetDbScriptsFromClass();

    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 9,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                    [OwnerId] UNIQUEIDENTIFIER NOT NULL,
                    [PasswordHash] NVARCHAR(256) NOT NULL,
                    [PasswordSalt] NVARCHAR(256) NOT NULL,
                    [Hostname] NVARCHAR(256) NULL,
                    [FriendlyName] NVARCHAR(256) NULL,
                    [Description] NVARCHAR(2048) NULL,
                    [PrivateIp] NVARCHAR(128) NULL,
                    [PublicIp] NVARCHAR(128) NULL,
                    [CurrentState] INT NOT NULL,
                    [Os] INT NOT NULL,
                    [OsName] NVARCHAR(1024) NULL,
                    [OsVersion] NVARCHAR(1024) NULL,
                    [AllowedPorts] VARBINARY(4096) NULL,
                    [Cpus] VARBINARY(4096) NULL,
                    [Motherboards] VARBINARY(4096) NULL,
                    [Storage] VARBINARY(4096) NULL,
                    [NetworkInterfaces] VARBINARY(4096) NULL,
                    [RamModules] VARBINARY(4096) NULL,
                    [Gpus] VARBINARY(4096) NULL,
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
                @DeletedBy UNIQUEIDENTIFIER,
                @DeletedOn datetime2
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET IsDeleted = 1, DeletedOn = @DeletedOn, LastModifiedBy = @DeletedBy
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
                FROM dbo.[{Table.TableName}] h
                WHERE h.IsDeleted = 0
                ORDER BY h.FriendlyName;
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
                WHERE h.IsDeleted = 0 AND h.CurrentState != 1
                ORDER BY h.FriendlyName ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                WHERE h.Hostname = @Hostname AND h.IsDeleted = 0
                ORDER BY h.Id;
            end"
    };

    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @Id UNIQUEIDENTIFIER,
                @OwnerId UNIQUEIDENTIFIER,
                @PasswordHash NVARCHAR(256),
                @PasswordSalt NVARCHAR(256),
                @Hostname NVARCHAR(256),
                @FriendlyName NVARCHAR(256),
                @Description NVARCHAR(2048),
                @PrivateIp NVARCHAR(128),
                @PublicIp NVARCHAR(128),
                @CurrentState INT,
                @Os INT,
                @OsName NVARCHAR(1024),
                @OsVersion NVARCHAR(1024),
                @AllowedPorts VARBINARY(4096),
                @Cpus VARBINARY(4096),
                @Motherboards VARBINARY(4096),
                @Storage VARBINARY(4096),
                @NetworkInterfaces VARBINARY(4096),
                @RamModules VARBINARY(4096),
                @Gpus VARBINARY(4096),
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn datetime2,
                @LastModifiedBy UNIQUEIDENTIFIER,
                @LastModifiedOn datetime2,
                @IsDeleted BIT,
                @DeletedOn datetime2
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (Id, OwnerId, PasswordHash, PasswordSalt, Hostname, FriendlyName, Description, PrivateIp, PublicIp, CurrentState, Os,
                                                     OsName, OsVersion, AllowedPorts, Cpus, Motherboards, Storage, NetworkInterfaces, RamModules, Gpus, CreatedBy, CreatedOn,
                                                     LastModifiedBy, LastModifiedOn, IsDeleted, DeletedOn)
                OUTPUT INSERTED.Id
                VALUES (@Id, @OwnerId, @PasswordHash, @PasswordSalt, @Hostname, @FriendlyName, @Description, @PrivateIp, @PublicIp, @CurrentState, @Os,
                        @OsName, @OsVersion, @AllowedPorts, @Cpus, @Motherboards, @Storage, @NetworkInterfaces, @RamModules, @Gpus,
                        @CreatedBy, @CreatedOn, @LastModifiedBy, @LastModifiedOn, @IsDeleted, @DeletedOn);
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
                WHERE h.IsDeleted = 0 AND h.CurrentState != 1
                    AND (h.Id LIKE '%' + @SearchTerm + '%'
                    OR h.Hostname LIKE '%' + @SearchTerm + '%'
                    OR h.FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR h.Description LIKE '%' + @SearchTerm + '%'
                    OR h.PrivateIp LIKE '%' + @SearchTerm + '%'
                    OR h.PublicIp LIKE '%' + @SearchTerm + '%')
                ORDER BY h.Id;
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
                WHERE h.IsDeleted = 0 AND h.CurrentState != 1
                    AND (h.Id LIKE '%' + @SearchTerm + '%'
                    OR h.Hostname LIKE '%' + @SearchTerm + '%'
                    OR h.FriendlyName LIKE '%' + @SearchTerm + '%'
                    OR h.Description LIKE '%' + @SearchTerm + '%'
                    OR h.PrivateIp LIKE '%' + @SearchTerm + '%'
                    OR h.PublicIp LIKE '%' + @SearchTerm + '%')
                ORDER BY h.FriendlyName ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                @Description NVARCHAR(2048) = null,
                @PrivateIp NVARCHAR(128) = null,
                @PublicIp NVARCHAR(128) = null,
                @CurrentState INT = null,
                @Os INT = null,
                @OsName NVARCHAR(1024) = null,
                @OsVersion NVARCHAR(1024) = null,
                @AllowedPorts VARBINARY(4096) = null,
                @Cpus VARBINARY(4096) = null,
                @Motherboards VARBINARY(4096) = null,
                @Storage VARBINARY(4096) = null,
                @NetworkInterfaces VARBINARY(4096) = null,
                @RamModules VARBINARY(4096) = null,
                @Gpus VARBINARY(4096) = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn datetime2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn datetime2 = null,
                @IsDeleted BIT = null,
                @DeletedOn datetime2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET OwnerId = COALESCE(@OwnerId, OwnerId), PasswordHash = COALESCE(@PasswordHash, PasswordHash), PasswordSalt = COALESCE(@PasswordSalt, PasswordSalt),
                    Hostname = COALESCE(@Hostname, Hostname), FriendlyName = COALESCE(@FriendlyName, FriendlyName), Description = COALESCE(@Description, Description),
                    PrivateIp = COALESCE(@PrivateIp, PrivateIp), PublicIp = COALESCE(@PublicIp, PublicIp), CurrentState = COALESCE(@CurrentState, CurrentState),
                    Os = COALESCE(@Os, Os), OsName = COALESCE(@OsName, OsName), OsVersion = COALESCE(@OsVersion, OsVersion),
                    AllowedPorts = COALESCE(@AllowedPorts, AllowedPorts), Cpus = COALESCE(@Cpus, Cpus), Motherboards = COALESCE(@Motherboards, Motherboards),
                    Storage = COALESCE(@Storage, Storage), NetworkInterfaces = COALESCE(@NetworkInterfaces, NetworkInterfaces), RamModules = COALESCE(@RamModules, RamModules),
                    Gpus = COALESCE(@Gpus, Gpus), CreatedBy = COALESCE(@CreatedBy, CreatedBy), CreatedOn = COALESCE(@CreatedOn, CreatedOn),
                    LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy), LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn),
                    IsDeleted = COALESCE(@IsDeleted, IsDeleted), DeletedOn = COALESCE(@DeletedOn, DeletedOn)
                WHERE Id = @Id;
            end"
    };

    public static readonly SqlStoredProcedure DeleteUnregisteredOlderThan = new()
    {
        Table = Table,
        Action = "DeleteUnregisteredOlderThan",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_DeleteUnregisteredOlderThan]
                @OlderThan DATETIME2
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
                WHERE CurrentState = 1
                    AND CreatedOn < @OlderThan;
            end"
    };
}