using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class ModsTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "Mods";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(ModsTablePgSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 10,
        TableName = TableName,
        SqlStatement = $@"
            CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                ""Id"" UUID PRIMARY KEY,
                ""GameId"" UUID NOT NULL,
                ""SteamGameId"" INT NOT NULL,
                ""SteamToolId"" INT NOT NULL,
                ""SteamId"" VARCHAR(128) NOT NULL,
                ""FriendlyName"" VARCHAR(128) NOT NULL,
                ""CurrentHash"" VARCHAR(128) NOT NULL,
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
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Delete"" (
                p_Id UUID,
                p_DeletedBy UUID,
                p_DeletedOn TIMESTAMP
            )
            LANGUAGE plpgsql 
            AS $$ $$
            BEGIN
                UPDATE ""{Table.TableName}""
                SET ""IsDeleted"" = TRUE, 
                    ""DeletedOn"" = p_DeletedOn, 
                    ""LastModifiedBy"" = p_DeletedBy
                WHERE ""Id"" = p_Id;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetAll = new()
    {
        Table = Table,
        Action = "GetAll",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAll"" (
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$ $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""IsDeleted"" = FALSE
                ORDER BY ""FriendlyName"" ASC;
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetAllPaginated = new()
    {
        Table = Table,
        Action = "GetAllPaginated",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAllPaginated"" (
                p_Offset INT,
                p_PageSize INT,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$ $$
            BEGIN
                OPEN p_ FOR
                SELECT COUNT(*) OVER() AS $$ ""TotalCount"", *
                FROM ""{Table.TableName}""
                WHERE ""IsDeleted"" = FALSE
                ORDER BY ""FriendlyName"" ASC 
                OFFSET p_Offset LIMIT p_PageSize;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetById = new()
    {
        Table = Table,
        Action = "GetById",
        SqlStatement = @$"
            CREATE OR ALTER REPLACE ""sp{Table.TableName}_GetById"" (
                p_Id UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$ $$
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
    
    public static readonly SqlStoredProcedure GetByGameId = new()
    {
        Table = Table,
        Action = "GetByGameId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByGameId]
                @GameId UUID
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
    
    public static readonly SqlStoredProcedure GetBySteamGameId = new()
    {
        Table = Table,
        Action = "GetBySteamGameId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetBySteamGameId"" (
                p_SteamGameId INT,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                SELECT m.*
                FROM ""{Table.TableName}""
                WHERE ""SteamGameId"" = p_SteamGameId AND ""IsDeleted"" = FALSE
                ORDER BY ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetBySteamToolId = new()
    {
        Table = Table,
        Action = "GetBySteamToolId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetBySteamToolId"" (
                p_SteamToolId INT,
                INOUT p REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""SteamToolId"" = p_SteamToolId AND ""IsDeleted"" = FALSE
                ORDER BY ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetBySteamId = new()
    {
        Table = Table,
        Action = "GetBySteamId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetBySteamId"" (
                p_SteamId VARCHAR(128)
                INOUT p REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""SteamId"" = p_SteamId AND ""IsDeleted"" = FALSE
                ORDER BY ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByFriendlyName = new()
    {
        Table = Table,
        Action = "GetByFriendlyName",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByFriendlyName"" (
                p_FriendlyName VARCHAR(128)
                INOUT p REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""FriendlyName"" = p_FriendlyName AND ""IsDeleted"" = FALSE
                ORDER BY ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByCurrentHash = new()
    {
        Table = Table,
        Action = "GetByCurrentHash",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByCurrentHash"" (
                p_CurrentHash VARCHAR(128),
                INOUT p REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""CurrentHash"" = p_CurrentHash
                ORDER BY ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Insert"" (
                p_GameId UUID,
                p_SteamGameId INT,
                p_SteamToolId INT,
                p_SteamId VARCHAR(128),
                p_FriendlyName VARCHAR(128),
                p_CurrentHash VARCHAR(128),
                p_CreatedBy UUID,
                p_CreatedOn TIMESTAMP,
                p_LastModifiedBy UUID,
                p_LastModifiedOn TIMESTAMP,
                p_IsDeleted BOOLEAN,
                p_DeletedOn TIMESTAMP,
                INOUT p REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                INSERT INTO ""{Table.TableName}"" (
                    ""GameId"", ""SteamGameId"", ""SteamToolId"", ""SteamId"", 
                    ""FriendlyName"", ""CurrentHash"", ""CreatedBy"", ""CreatedOn"", 
                    ""LastModifiedBy"", ""LastModifiedOn"", ""IsDeleted"", ""DeletedOn""
                )
                VALUES (
                    p_GameId, p_SteamGameId, p_SteamToolId, p_SteamId, p_FriendlyName, 
                    p_CurrentHash, p_CreatedBy, p_CreatedOn, p_LastModifiedBy, 
                    p_LastModifiedOn, p_IsDeleted, p_DeletedOn
                )
                RETURNING ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure Search = new()
    {
        Table = Table,
        Action = "Search",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Search"" (
                p_SearchTerm VARCHAR(256),
                INOUT p REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR 
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""IsDeleted"" = FALSE
                    AND (
                        ""Id"" LIKE '%' || p_SearchTerm || '%'
                        OR ""GameId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamGameId"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamToolId"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamId"" LIKE '%' || p_SearchTerm || '%'
                        OR ""FriendlyName"" LIKE '%' || p_SearchTerm || '%'
                    );
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure SearchPaginated = new()
    {
        Table = Table,
        Action = "SearchPaginated",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_SearchPaginated]
                p_SearchTerm VARCHAR(256),
                p_Offset INT,
                p_PageSize INT,
                INOUT p REFCURSOR
            LANGUAGE plpgsql
            AS $$
            BEGIN
                SELECT COUNT(*) OVER() AS ""TotalCount"", *
                FROM ""{Table.TableName}""
                WHERE ""IsDeleted"" = FALSE
                    AND (
                        ""Id""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""GameId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamGameId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamToolId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamId"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""FriendlyName"" LIKE '%' || p_SearchTerm || '%'
                    )
                ORDER BY ""FriendlyName"" ASC 
                OFFSET p_Offset LIMIT p_PageSize;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Update"" (
                p_Id UUID,
                p_GameId UUID DEFAULT NULL,
                p_SteamGameId INT DEFAULT NULL,
                p_SteamToolId INT DEFAULT NULL,
                p_SteamId VARCHAR(128) DEFAULT NULL,
                p_FriendlyName VARCHAR(128) DEFAULT NULL,
                p_CurrentHash VARCHAR(128) DEFAULT NULL,
                p_CreatedBy UUID DEFAULT NULL,
                p_CreatedOn TIMESTAMP DEFAULT NULL,
                p_LastModifiedBy UUID DEFAULT NULL,
                p_LastModifiedOn TIMESTAMP DEFAULT NULL,
                p_IsDeleted BOOLEAN DEFAULT NULL,
                p_DeletedOn TIMESTAMP DEFAULT NULL,
                INOUT p REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE ""{Table.TableName}""
                SET 
                    ""GameId"" = COALESCE(p_GameId, ""GameId""), 
                    ""SteamGameId"" = COALESCE(p_SteamGameId, ""SteamGameId""), 
                    ""SteamToolId"" = COALESCE(p_SteamToolId, ""SteamToolId""),
                    ""SteamId"" = COALESCE(p_SteamId, ""SteamId""), 
                    ""FriendlyName"" = COALESCE(p_FriendlyName, ""FriendlyName""), 
                    ""CurrentHash"" = COALESCE(p_CurrentHash, ""CurrentHash""),
                    ""CreatedBy"" = COALESCE(p_CreatedBy, ""CreatedBy""), 
                    ""CreatedOn"" = COALESCE(p_CreatedOn, ""CreatedOn""), 
                    ""LastModifiedBy"" = COALESCE(p_LastModifiedBy, ""LastModifiedBy""),
                    ""LastModifiedOn"" = COALESCE(p_LastModifiedOn, ""LastModifiedOn""), 
                    ""IsDeleted"" = COALESCE(p_IsDeleted, ""IsDeleted""), 
                    ""DeletedOn"" = COALESCE(p_DeletedOn, ""DeletedOn"")
                WHERE ""Id"" = p_Id;
            END;
            $$;"
    };
}