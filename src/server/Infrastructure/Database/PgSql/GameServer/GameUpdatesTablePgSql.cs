using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class GameUpdatesTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "GameUpdates";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(GameUpdatesTablePgSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 10,
        TableName = TableName,
        SqlStatement = $@"
            CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                ""Id"" UUID PRIMARY KEY,
                ""GameId"" UUID NOT NULL,
                ""SupportsWindows"" BOOLEAN NOT NULL,
                ""SupportsLinux"" BOOLEAN NOT NULL,
                ""SupportsMac"" BOOLEAN NOT NULL,
                ""BuildVersion"" VARCHAR(128) NOT NULL,
                ""BuildVersionReleased"" TIMESTAMP NOT NULL
            );"
    };
    
    public static readonly SqlStoredProcedure Delete = new()
    {
        Table = Table,
        Action = "Delete",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Delete"" (
                IN p_Id UUID
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                DELETE FROM ""{Table.TableName}""
                WHERE ""Id"" = p_Id;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure DeleteForGameId = new()
    {
        Table = Table,
        Action = "DeleteForGameId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_DeleteForGameId"" (
                IN p_Id UUID
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                DELETE FROM ""{Table.TableName}""
                WHERE ""GameId"" = p_Id;
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
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                ORDER BY ""BuildVersionReleased"" DESC;
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetAllPaginated = new()
    {
        Table = Table,
        Action = "GetAllPaginated",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAllPaginated"" (
                IN p_Offset INT,
                IN p_PageSize INT,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT COUNT(*) OVER() AS ""TotalCount"", *
                FROM ""{Table.TableName}""
                ORDER BY ""BuildVersionReleased"" DESC
                OFFSET p_Offset LIMIT p_PageSize;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetById = new()
    {
        Table = Table,
        Action = "GetById",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetById"" (
                IN p_Id UUID,
                INOUT p_ REFCURSOR
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
    
    public static readonly SqlStoredProcedure GetByGameId = new()
    {
        Table = Table,
        Action = "GetByGameId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByGameId"" (
                IN p_Id UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""GameId"" = p_Id
                ORDER BY ""BuildVersionReleased"" DESC
                LIMIT 1;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Insert"" (
                IN p_GameId UUID,
                IN p_SupportsWindows BOOLEAN,
                IN p_SupportsLinux BOOLEAN,
                IN p_SupportsMac BOOLEAN,
                IN p_BuildVersion VARCHAR(128),
                IN p_BuildVersionReleased TIMESTAMP,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                WITH inserted AS (
                    INSERT INTO ""{{Table.TableName}}"" (
                        ""GameId"", ""SupportsWindows"", ""SupportsLinux"", ""SupportsMac"", ""BuildVersion"", ""BuildVersionReleased""
                    )
                    VALUES (
                        p_GameId, p_SupportsWindows, p_SupportsLinux, p_SupportsMac, p_BuildVersion, p_BuildVersionReleased
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
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Search"" (
                IN p_SearchTerm VARCHAR(256),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR 
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Id""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""GameId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""BuildVersion"" ILIKE '%' || p_SearchTerm || '%'
            ORDER BY ""BuildVersionReleased"" DESC;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure SearchPaginated = new()
    {
        Table = Table,
        Action = "SearchPaginated",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_SearchPaginated"" (
                IN p_SearchTerm VARCHAR(256),
                IN p_Offset INT,
                IN p_PageSize INT,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT COUNT(*) OVER() AS ""TotalCount"", *
                FROM ""{Table.TableName}""
                WHERE ""Id""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""GameId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""BuildVersion"" ILIKE '%' || p_SearchTerm || '%'
                ORDER BY ""BuildVersionReleased"" DESC 
                OFFSET p_Offset LIMIT p_PageSize;
            END;
            $$;"
    };
}