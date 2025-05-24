using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class GameServersTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "GameServers";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(GameServersTablePgSql).GetDbScriptsFromClass();

    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 7,
        TableName = TableName,
        SqlStatement = $@"
            CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                ""Id"" UUID PRIMARY KEY,
                ""OwnerId"" UUID NOT NULL,
                ""HostId"" UUID NOT NULL,
                ""GameId"" UUID NOT NULL,
                ""GameProfileId"" UUID NOT NULL,
                ""ParentGameProfileId"" UUID NULL,
                ""ServerBuildVersion"" VARCHAR(128) NOT NULL,
                ""ServerName"" VARCHAR(128) NOT NULL,
                ""Password"" VARCHAR(128) NOT NULL,
                ""PasswordRcon"" VARCHAR(128) NOT NULL,
                ""PasswordAdmin"" VARCHAR(128) NOT NULL,
                ""PublicIp"" VARCHAR(128) NOT NULL,
                ""PrivateIp"" VARCHAR(128) NOT NULL,
                ""ExternalHostname"" VARCHAR(128) NOT NULL,
                ""PortGame"" INT NOT NULL,
                ""PortPeer"" INT NOT NULL,
                ""PortQuery"" INT NOT NULL,
                ""PortRcon"" INT NOT NULL,
                ""Modded"" BOOLEAN NOT NULL,
                ""Private"" BOOLEAN NOT NULL,
                ""ServerState"" INT NOT NULL,
                ""CreatedBy"" UUID NOT NULL,
                ""CreatedOn"" TIMESTAMP NOT NULL,
                ""LastModifiedBy"" UUID NULL,
                ""LastModifiedOn"" TIMESTAMP NULL,
                ""IsDeleted"" BOOLEAN NOT NULL,
                ""DeletedOn"" TIMESTAMP NULL
            );"
    };

    public static readonly SqlStoredProcedure Delete = new()
    {
        Table = Table,
        Action = "Delete",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Delete""(
                p_Id UUID,
                p_DeletedBy UUID,
                p_DeletedOn TIMESTAMP
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE ""{Table.TableName}""
                SET ""IsDeleted"" = TRUE, ""DeletedOn"" = p_DeletedOn, ""LastModifiedBy"" = p_DeletedBy
                WHERE ""Id"" = p_Id;
            END;
            $$"
    };

    public static readonly SqlStoredProcedure GetAll = new()
    {
        Table = Table,
        Action = "GetAll",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAll"" (
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT g
                FROM ""{Table.TableName}""
                WHERE ""IsDeleted"" = False
                ORDER BY ""ServerName"" ASC;
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetAllPaginated = new()
    {
        Table = Table,
        Action = "GetAllPaginated",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAllPaginated""(
                p_Offset INT,
                p_PageSize INT
                INOUT p_ REFCURSOR DEFAULT NULL
                )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT COUNT(*) OVER() AS ""TotalCount"", *
                FROM ""{Table.TableName}""
                WHERE ""IsDeleted"" = FALSE
                ORDER BY ""ServerName"" ASC
                OFFSET p_Offset LIMIT p_PageSize;
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetById = new()
    {
        Table = Table,
        Action = "GetById",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetById""(
                p_Id UUID,
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR     
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Id"" = p_Id
                ORDER BY ""Id""
                LIMIT 1;
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetByOwnerId = new()
    {
        Table = Table,
        Action = "GetByOwnerId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByOwnerId""(
                p_OwnerId UUID,
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""OwnerId"" = p_OwnerId AND ""IsDeleted"" = FALSE
                ORDER BY ""Id"";
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetByHostId = new()
    {
        Table = Table,
        Action = "GetByHostId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByHostId""(
                p_HostId UUID,
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""HostId"" = p_HostId AND ""IsDeleted"" = FALSE
                ORDER BY ""Id"";
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetByGameId = new()
    {
        Table = Table,
        Action = "GetByGameId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByGameId""(
                p_GameId UUID,
                INOUT p_ REFCURSOR DEFAULT NULL
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""GameId"" = p_GameId AND ""IsDeleted"" = FALSE
                ORDER BY ""Id"";
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetByGameProfileId = new()
    {
        Table = Table,
        Action = "GetByGameProfileId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByGameProfileId""(
                p_GameProfileId UUID
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""GameProfileId"" = p_GameProfileId
                    AND ""IsDeleted"" = FALSE
                ORDER BY ""Id"";
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetByParentGameProfileId = new()
    {
        Table = Table,
        Action = "GetByParentGameProfileId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByParentGameProfileId"" (
                p_ParentGameProfileId UUID,
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIn
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""ParentGameProfileId"" = p_ParentGameProfileId
                    AND ""IsDeleted"" = 0
                ORDER BY ""Id"";
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetByServerBuildVersion = new()
    {
        Table = Table,
        Action = "GetByServerBuildVersion",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByServerBuildVersion""(
                p_ServerBuildVersion VARCHAR(128),
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""ServerBuildVersion"" = p_ServerBuildVersion
                    AND ""IsDeleted"" = FALSE
                ORDER BY ""Id"";
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetByServerName = new()
    {
        Table = Table,
        Action = "GetByServerName",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByServerName""(
                @ServerName VARCHAR(128),
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""ServerName"" = p_ServerName AND ""IsDeleted"" = FALSE
                ORDER BY ""Id"";
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Insert""(
                IN p_OwnerId UUID,
                IN p_HostId UUID,
                IN p_GameId UUID,
                IN p_GameProfileId UUID,
                IN p_ParentGameProfileId UUID,
                IN p_ServerBuildVersion VARCHAR(128),
                IN p_ServerName VARCHAR(128),
                IN p_Password VARCHAR(128),
                IN p_PasswordRcon VARCHAR(128),
                IN p_PasswordAdmin VARCHAR(128),
                IN p_PublicIp VARCHAR(128),
                IN p_PrivateIp VARCHAR(128),
                IN p_ExternalHostname VARCHAR(128),
                IN p_PortGame INT,
                IN p_PortPeer INT,
                IN p_PortQuery INT,
                IN p_PortRcon INT,
                IN p_Modded BOOLEAN,
                IN p_Private BOOLEAN,
                IN p_ServerState INT,
                IN p_CreatedBy UUID,
                IN p_CreatedOn TIMESTAMP,
                IN p_IsDeleted BOOLEAN,
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                WITH inserted AS (
                    INSERT INTO ""{Table.TableName}"" (
                        ""OwnerId"", ""HostId"", ""GameId"", ""GameProfileId"", ""ParentGameProfileId"", ""ServerBuildVersion"", ""ServerName"", ""Password"", ""PasswordRcon"",
                        ""PasswordAdmin"", ""PublicIp"", ""PrivateIp"", ""ExternalHostname"", ""PortGame"", ""PortPeer"", ""PortQuery"", ""PortRcon"", ""Modded"", ""Private"", ""ServerState"",
                        ""CreatedBy"", ""CreatedOn"", ""IsDeleted""
                    )
                    VALUES (
                        p_OwnerId, p_HostId, p_GameId, p_GameProfileId, p_ParentGameProfileId, p_ServerBuildVersion, p_ServerName, p_Password, p_PasswordRcon,
                        p_PasswordAdmin, p_PublicIp, p_PrivateIp, p_ExternalHostname, p_PortGame, p_PortPeer, p_PortQuery, p_PortRcon, p_Modded, p_Private, p_ServerState,
                        p_CreatedBy, p_CreatedOn, p_IsDeleted
                    )
                    RETURNING ""Id""
                )
                SELECT * FROM inserted;
            END;
            $$;"
    };


    public static readonly SqlStoredProcedure Search = new()
    {
        Table = Table,
        Action = "Search",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE ""sp{Table.TableName}_Search""(
                IN p_SearchTerm VARCHAR(256),
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                SET nocount on;
                
                SELECT g.*
                FROM dbo.[{Table.TableName}] g
                WHERE g.IsDeleted = 0
                    AND (g.Id LIKE '%' + @SearchTerm + '%'
                    OR g.OwnerId LIKE '%' + @SearchTerm + '%'
                    OR g.HostId LIKE '%' + @SearchTerm + '%'
                    OR g.GameId LIKE '%' + @SearchTerm + '%'
                    OR g.GameProfileId LIKE '%' + @SearchTerm + '%'
                    OR g.ParentGameProfileId LIKE '%' + @SearchTerm + '%'
                    OR g.ServerBuildVersion LIKE '%' + @SearchTerm + '%'
                    OR g.PublicIp LIKE '%' + @SearchTerm + '%'
                    OR g.PrivateIp LIKE '%' + @SearchTerm + '%'
                    OR g.ExternalHostname LIKE '%' + @SearchTerm + '%'
                    OR g.ServerName LIKE '%' + @SearchTerm + '%');
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure SearchPaginated = new()
    {
        Table = Table,
        Action = "SearchPaginated",
        SqlStatement = @$"
        CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_SearchPaginated""(
            IN p_SearchTerm VARCHAR(256),
            IN p_Offset INT,
            IN p_PageSize INT,
            INOUT p_ REFCURSOR DEFAULT NULL
        )
        LANGUAGE plpgsql
        AS $$
        BEGIN
            OPEN p_ FOR
            SELECT COUNT(*) OVER() AS ""TotalCount"", *
            FROM ""{Table.TableName}""
            WHERE ""IsDeleted"" = FALSE
                AND (
                    ""Id""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""OwnerId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""HostId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""GameId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""GameProfileId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""ParentGameProfileId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""ServerBuildVersion"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""PublicIp"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""PrivateIp"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""ExternalHostname"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""ServerName"" ILIKE '%' || p_SearchTerm || '%'
                )
            ORDER BY ""ServerName"" ASC
            OFFSET p_Offset LIMIT p_PageSize;
        END;
        $$;"
    };


    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Update""(
                IN p_Id UUID,
                IN p_OwnerId UUID DEFAULT NULL,
                IN p_HostId UUID DEFAULT NULL,
                IN p_GameId UUID DEFAULT NULL,
                IN p_GameProfileId UUID DEFAULT NULL,
                IN p_ParentGameProfileId UUID DEFAULT NULL,
                IN p_ServerBuildVersion VARCHAR(128) DEFAULT NULL,
                IN p_ServerName VARCHAR(128) DEFAULT NULL,
                IN p_Password VARCHAR(128) DEFAULT NULL,
                IN p_PasswordRcon VARCHAR(128) DEFAULT NULL,
                IN p_PasswordAdmin VARCHAR(128) DEFAULT NULL,
                IN p_PublicIp VARCHAR(128) DEFAULT NULL,
                IN p_PrivateIp VARCHAR(128) DEFAULT NULL,
                IN p_ExternalHostname VARCHAR(128) DEFAULT NULL,
                IN p_PortGame INT DEFAULT NULL,
                IN p_PortPeer INT DEFAULT NULL,
                IN p_PortQuery INT DEFAULT NULL,
                IN p_PortRcon INT DEFAULT NULL,
                IN p_Modded BOOLEAN DEFAULT NULL,
                IN p_Private BOOLEAN DEFAULT NULL,
                IN p_ServerState INT DEFAULT NULL,
                IN p_CreatedBy UUID DEFAULT NULL,
                IN p_CreatedOn TIMESTAMP DEFAULT NULL,
                IN p_LastModifiedBy UUID DEFAULT NULL,
                IN p_LastModifiedOn TIMESTAMP DEFAULT NULL,
                IN p_IsDeleted BOOLEAN DEFAULT NULL,
                IN p_DeletedOn TIMESTAMP DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE ""{Table.TableName}""
                SET
                    ""OwnerId""            = COALESCE(p_OwnerId, ""OwnerId""), 
                    ""HostId""             = COALESCE(p_HostId, ""HostId""), 
                    ""GameId""             = COALESCE(p_GameId, ""GameId""), 
                    ""GameProfileId""      = COALESCE(p_GameProfileId, ""GameProfileId""), 
                    ""ParentGameProfileId""= COALESCE(p_ParentGameProfileId, ""ParentGameProfileId""), 
                    ""ServerBuildVersion"" = COALESCE(p_ServerBuildVersion, ""ServerBuildVersion""), 
                    ""ServerName""         = COALESCE(p_ServerName, ""ServerName""), 
                    ""Password""           = COALESCE(p_Password, ""Password""), 
                    ""PasswordRcon""       = COALESCE(p_PasswordRcon, ""PasswordRcon""), 
                    ""PasswordAdmin""      = COALESCE(p_PasswordAdmin, ""PasswordAdmin""), 
                    ""PublicIp""           = COALESCE(p_PublicIp, ""PublicIp""), 
                    ""PrivateIp""          = COALESCE(p_PrivateIp, ""PrivateIp""), 
                    ""ExternalHostname""   = COALESCE(p_ExternalHostname, ""ExternalHostname""), 
                    ""PortGame""           = COALESCE(p_PortGame, ""PortGame""), 
                    ""PortPeer""           = COALESCE(p_PortPeer, ""PortPeer""), 
                    ""PortQuery""          = COALESCE(p_PortQuery, ""PortQuery""), 
                    ""PortRcon""           = COALESCE(p_PortRcon, ""PortRcon""), 
                    ""Modded""             = COALESCE(p_Modded, ""Modded""), 
                    ""Private""            = COALESCE(p_Private, ""Private""), 
                    ""ServerState""        = COALESCE(p_ServerState, ""ServerState""), 
                    ""CreatedBy""          = COALESCE(p_CreatedBy, ""CreatedBy""), 
                    ""CreatedOn""          = COALESCE(p_CreatedOn, ""CreatedOn""), 
                    ""LastModifiedBy""     = COALESCE(p_LastModifiedBy, ""LastModifiedBy""), 
                    ""LastModifiedOn""     = COALESCE(p_LastModifiedOn, ""LastModifiedOn""), 
                    ""IsDeleted""          = COALESCE(p_IsDeleted, ""IsDeleted""), 
                    ""DeletedOn""          = COALESCE(p_DeletedOn, ""DeletedOn"")
                WHERE ""Id"" = p_Id;
            END;
            $$;"
    };
}