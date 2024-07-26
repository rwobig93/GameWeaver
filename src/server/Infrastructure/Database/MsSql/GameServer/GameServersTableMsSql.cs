using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.GameServer;

public class GameServersTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "GameServers";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(GameServersTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 7,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [OwnerId] UNIQUEIDENTIFIER NOT NULL,
                    [HostId] UNIQUEIDENTIFIER NOT NULL,
                    [GameId] UNIQUEIDENTIFIER NOT NULL,
                    [GameProfileId] UNIQUEIDENTIFIER NOT NULL,
                    [ParentGameProfileId] UNIQUEIDENTIFIER NULL,
                    [ServerBuildVersion] NVARCHAR(128) NOT NULL,
                    [ServerName] NVARCHAR(128) NOT NULL,
                    [Password] NVARCHAR(128) NOT NULL,
                    [PasswordRcon] NVARCHAR(128) NOT NULL,
                    [PasswordAdmin] NVARCHAR(128) NOT NULL,
                    [PublicIp] NVARCHAR(128) NOT NULL,
                    [PrivateIp] NVARCHAR(128) NOT NULL,
                    [ExternalHostname] NVARCHAR(128) NOT NULL,
                    [PortGame] INT NOT NULL,
                    [PortPeer] INT NOT NULL,
                    [PortQuery] INT NOT NULL,
                    [PortRcon] INT NOT NULL,
                    [Modded] BIT NOT NULL,
                    [Private] BIT NOT NULL,
                    [ServerState] INT NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [CreatedOn] DATETIME2 NOT NULL,
                    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
                    [LastModifiedOn] DATETIME2 NULL,
                    [IsDeleted] BIT NOT NULL,
                    [DeletedOn] DATETIME2 NULL
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
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0
                ORDER BY g.ServerName ASC;
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
                SELECT COUNT(*) OVER() AS TotalCount, g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0
                ORDER BY g.ServerName ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                WHERE g.OwnerId = @OwnerId AND g.IsDeleted = 0
                ORDER BY g.Id;
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
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.HostId = @HostId AND g.IsDeleted = 0
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
                WHERE g.GameId = @GameId AND g.IsDeleted = 0
                ORDER BY g.Id;
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
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.GameProfileId = @GameProfileId
                    AND g.IsDeleted = 0
                ORDER BY g.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByParentGameProfileId = new()
    {
        Table = Table,
        Action = "GetByParentGameProfileId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByParentGameProfileId]
                @ParentGameProfileId UNIQUEIDENTIFIER
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.ParentGameProfileId = @ParentGameProfileId
                    AND g.IsDeleted = 0
                ORDER BY g.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByServerBuildVersion = new()
    {
        Table = Table,
        Action = "GetByServerBuildVersion",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByServerBuildVersion]
                @ServerBuildVersion NVARCHAR(128)
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.ServerBuildVersion = @ServerBuildVersion
                    AND g.IsDeleted = 0
                ORDER BY g.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByServerName = new()
    {
        Table = Table,
        Action = "GetByServerName",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByServerName]
                @ServerName NVARCHAR(128)
            AS
            begin
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.ServerName = @ServerName AND g.IsDeleted = 0
                ORDER BY g.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @OwnerId UNIQUEIDENTIFIER,
                @HostId UNIQUEIDENTIFIER,
                @GameId UNIQUEIDENTIFIER,
                @GameProfileId UNIQUEIDENTIFIER,
                @ParentGameProfileId UNIQUEIDENTIFIER,
                @ServerBuildVersion NVARCHAR(128),
                @ServerName NVARCHAR(128),
                @Password NVARCHAR(128),
                @PasswordRcon NVARCHAR(128),
                @PasswordAdmin NVARCHAR(128),
                @PublicIp NVARCHAR(128),
                @PrivateIp NVARCHAR(128),
                @ExternalHostname NVARCHAR(128),
                @PortGame INT,
                @PortPeer INT,
                @PortQuery INT,
                @PortRcon INT,
                @Modded BIT,
                @Private BIT,
                @ServerState INT,
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn DATETIME2,
                @IsDeleted BIT
            AS
            begin
                INSERT into dbo.[{Table.TableName}]  (OwnerId, HostId, GameId, GameProfileId, ParentGameProfileId, ServerBuildVersion, ServerName, Password, PasswordRcon,
                                                      PasswordAdmin, PublicIp, PrivateIp, ExternalHostname, PortGame, PortPeer, PortQuery, PortRcon, Modded, Private, ServerState,
                                                      CreatedBy, CreatedOn, IsDeleted)
                OUTPUT INSERTED.Id
                VALUES (@OwnerId, @HostId, @GameId, @GameProfileId, @ParentGameProfileId, @ServerBuildVersion, @ServerName, @Password, @PasswordRcon, @PasswordAdmin,
                        @PublicIp, @PrivateIp, @ExternalHostname, @PortGame, @PortPeer, @PortQuery, @PortRcon, @Modded, @Private, @ServerState, @CreatedBy, @CreatedOn, @IsDeleted);
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
                WHERE g.IsDeleted = 0
                    AND g.Id LIKE '%' + @SearchTerm + '%'
                    OR g.OwnerId LIKE '%' + @SearchTerm + '%'
                    OR g.HostId LIKE '%' + @SearchTerm + '%'
                    OR g.GameId LIKE '%' + @SearchTerm + '%'
                    OR g.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR g.ParentGameProfileId LIKE '%' + @SearchTerm + '%'
                    OR g.ServerBuildVersion LIKE '%' + @SearchTerm + '%'
                    OR g.PublicIp LIKE '%' + @SearchTerm + '%'
                    OR g.PrivateIp LIKE '%' + @SearchTerm + '%'
                    OR g.ExternalHostname LIKE '%' + @SearchTerm + '%'
                    OR g.ServerName LIKE '%' + @SearchTerm + '%';
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
                SELECT COUNT(*) OVER() AS TotalCount, g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0
                    AND g.Id LIKE '%' + @SearchTerm + '%'
                    OR g.OwnerId LIKE '%' + @SearchTerm + '%'
                    OR g.HostId LIKE '%' + @SearchTerm + '%'
                    OR g.GameId LIKE '%' + @SearchTerm + '%'
                    OR g.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR g.ParentGameProfileId LIKE '%' + @SearchTerm + '%'
                    OR g.ServerBuildVersion LIKE '%' + @SearchTerm + '%'
                    OR g.PublicIp LIKE '%' + @SearchTerm + '%'
                    OR g.PrivateIp LIKE '%' + @SearchTerm + '%'
                    OR g.ExternalHostname LIKE '%' + @SearchTerm + '%'
                    OR g.ServerName LIKE '%' + @SearchTerm + '%'
                ORDER BY g.ServerName ASC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
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
                @HostId UNIQUEIDENTIFIER = null,
                @GameId UNIQUEIDENTIFIER = null,
                @GameProfileId UNIQUEIDENTIFIER = null,
                @ParentGameProfileId UNIQUEIDENTIFIER = null,
                @ServerBuildVersion NVARCHAR(128) = null,
                @ServerName NVARCHAR(128) = null,
                @Password NVARCHAR(128) = null,
                @PasswordRcon NVARCHAR(128) = null,
                @PasswordAdmin NVARCHAR(128) = null,
                @PublicIp NVARCHAR(128) = null,
                @PrivateIp NVARCHAR(128) = null,
                @ExternalHostname NVARCHAR(128) = null,
                @PortGame INT = null,
                @PortPeer INT = null,
                @PortQuery INT = null,
                @PortRcon INT = null,
                @Modded BIT = null,
                @Private BIT = null,
                @ServerState INT = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn DATETIME2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn DATETIME2 = null,
                @IsDeleted BIT = null,
                @DeletedOn DATETIME2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET OwnerId = COALESCE(@OwnerId, OwnerId), HostId = COALESCE(@HostId, HostId), GameId = COALESCE(@GameId, GameId),
                    GameProfileId = COALESCE(@GameProfileId, GameProfileId), ParentGameProfileId = COALESCE(@ParentGameProfileId, ParentGameProfileId),
                    ServerBuildVersion = COALESCE(@ServerBuildVersion, ServerBuildVersion), ServerName = COALESCE(@ServerName, ServerName),
                    Password = COALESCE(@Password, Password), PasswordRcon = COALESCE(@PasswordRcon, PasswordRcon), PasswordAdmin = COALESCE(@PasswordAdmin, PasswordAdmin),
                    PublicIp = COALESCE(@PublicIp, PublicIp), PrivateIp = COALESCE(@PrivateIp, PrivateIp), ExternalHostname = COALESCE(@ExternalHostname, ExternalHostname),
                    PortGame = COALESCE(@PortGame, PortGame), PortPeer = COALESCE(@PortPeer, PortPeer), PortQuery = COALESCE(@PortQuery, PortQuery),
                    PortRcon = COALESCE(@PortRcon, PortRcon), Modded = COALESCE(@Modded, Modded), Private = COALESCE(@Private, Private),
                    ServerState = COALESCE(@ServerState, ServerState), CreatedBy = COALESCE(@CreatedBy, CreatedBy), CreatedOn = COALESCE(@CreatedOn, CreatedOn),
                    LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy), LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn),
                    IsDeleted = COALESCE(@IsDeleted, IsDeleted), DeletedOn = COALESCE(@DeletedOn, DeletedOn)
                WHERE Id = @Id;
            end"
    };
}