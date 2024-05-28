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
        EnforcementOrder = 1,
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
                    [ServerName] NVARCHAR(128) NOT NULL,
                    [Password] NVARCHAR(128) NOT NULL,
                    [PasswordRcon] NVARCHAR(128) NOT NULL,
                    [PasswordAdmin] NVARCHAR(128) NOT NULL,
                    [PublicIp] NVARCHAR(128) NOT NULL,
                    [PrivateIp] NVARCHAR(128) NOT NULL,
                    [ExternalHostname] NVARCHAR(128) NOT NULL,
                    [PortGame] int NOT NULL,
                    [PortQuery] int NOT NULL,
                    [PortRcon] int NOT NULL,
                    [Modded] BIT NOT NULL,
                    [Private] BIT NOT NULL,
                    [ServerState] int NOT NULL,
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
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0
                ORDER BY g.Id;
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
                WHERE g.IsDeleted = 0
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
                @ServerName NVARCHAR(128),
                @Password NVARCHAR(128),
                @PasswordRcon NVARCHAR(128),
                @PasswordAdmin NVARCHAR(128),
                @PublicIp NVARCHAR(128),
                @PrivateIp NVARCHAR(128),
                @ExternalHostname NVARCHAR(128),
                @PortGame int,
                @PortQuery int,
                @PortRcon int,
                @Modded BIT,
                @Private BIT,
                @ServerState int,
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn datetime2,
                @IsDeleted BIT
            AS
            begin
                INSERT into dbo.[{Table.TableName}]  (OwnerId, HostId, GameId, GameProfileId, ServerName, Password, PasswordRcon, PasswordAdmin, PublicIp, PrivateIp,
                                                      ExternalHostname, PortGame, PortQuery, PortRcon, Modded, Private, ServerState, CreatedBy, CreatedOn, IsDeleted)
                OUTPUT INSERTED.Id
                VALUES (@OwnerId, @HostId, @GameId, @GameProfileId, @ServerName, @Password, @PasswordRcon, @PasswordAdmin, @PublicIp, @PrivateIp, @ExternalHostname, @PortGame,
                        @PortQuery, @PortRcon, @Modded, @Private, @ServerState, @CreatedBy, @CreatedOn, @IsDeleted);
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
                WHERE g.IsDeleted = 0 AND g.OwnerId LIKE '%' + @SearchTerm + '%'
                    OR g.HostId LIKE '%' + @SearchTerm + '%'
                    OR g.GameId LIKE '%' + @SearchTerm + '%'
                    OR g.GameProfileId LIKE '%' + @SearchTerm + '%'
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
                SET nocount on;
                
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0 AND g.OwnerId LIKE '%' + @SearchTerm + '%'
                    OR g.HostId LIKE '%' + @SearchTerm + '%'
                    OR g.GameId LIKE '%' + @SearchTerm + '%'
                    OR g.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR g.PublicIp LIKE '%' + @SearchTerm + '%'
                    OR g.PrivateIp LIKE '%' + @SearchTerm + '%'
                    OR g.ExternalHostname LIKE '%' + @SearchTerm + '%'
                    OR g.ServerName LIKE '%' + @SearchTerm + '%'
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
                @OwnerId UNIQUEIDENTIFIER = null,
                @HostId UNIQUEIDENTIFIER = null,
                @GameId UNIQUEIDENTIFIER = null,
                @GameProfileId UNIQUEIDENTIFIER = null,
                @ServerName NVARCHAR(128) = null,
                @Password NVARCHAR(128) = null,
                @PasswordRcon NVARCHAR(128) = null,
                @PasswordAdmin NVARCHAR(128) = null,
                @PublicIp NVARCHAR(128) = null,
                @PrivateIp NVARCHAR(128) = null,
                @ExternalHostname NVARCHAR(128) = null,
                @PortGame int = null,
                @PortQuery int = null,
                @PortRcon int = null,
                @Modded BIT = null,
                @Private BIT = null,
                @ServerState int = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn datetime2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn datetime2 = null,
                @IsDeleted BIT = null,
                @DeletedOn datetime2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET OwnerId = COALESCE(@OwnerId, OwnerId), HostId = COALESCE(@HostId, HostId), GameId = COALESCE(@GameId, GameId),
                    GameProfileId = COALESCE(@GameProfileId, GameProfileId), ServerName = COALESCE(@ServerName, ServerName), Password = COALESCE(@Password, Password),
                    PasswordRcon = COALESCE(@PasswordRcon, PasswordRcon), PasswordAdmin = COALESCE(@PasswordAdmin, PasswordAdmin), PublicIp = COALESCE(@PublicIp, PublicIp),
                    PrivateIp = COALESCE(@PrivateIp, PrivateIp), ExternalHostname = COALESCE(@ExternalHostname, ExternalHostname), PortGame = COALESCE(@PortGame, PortGame),
                    PortQuery = COALESCE(@PortQuery, PortQuery), PortRcon = COALESCE(@PortRcon, PortRcon), Modded = COALESCE(@Modded, Modded),
                    Private = COALESCE(@Private, Private), ServerState = COALESCE(@ServerState, ServerState), CreatedBy = COALESCE(@CreatedBy, CreatedBy),
                    CreatedOn = COALESCE(@CreatedOn, CreatedOn), LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy),
                    LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn), IsDeleted = COALESCE(@IsDeleted, IsDeleted), DeletedOn = COALESCE(@DeletedOn, DeletedOn)
                WHERE Id = @Id;
            end"
    };
}