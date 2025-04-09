using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class DevelopersTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "Developers";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(DevelopersTablePgSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 10,
        TableName = TableName,
        SqlStatement = $@"
            CREATE TABLE IF NOT EXISTS ""{TableName}""(
                ""Id"" UUID PRIMARY KEY,
                ""GameId"" UUID NOT NULL,
                ""Name"" VARCHAR(128) NOT NULL
            );"
    };
    
    public static readonly SqlStoredProcedure Delete = new()
    {
        Table = Table,
        Action = "Delete",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Delete"" (
                p_Id UUID
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                DELETE FROM ""{Table.TableName}""
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
            INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                ORDER BY ""Name"" ASC;
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
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT COUNT(*) OVER() AS ""TotalCount"", *
                FROM ""{Table.TableName}""
                ORDER BY ""Name"" ASC 
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
                p_Id UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Id"" = p_Id
                ORDER BY ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByGameId = new()
    {
        Table = Table,
        Action = "GetByGameId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByGameId"" (
                p_GameId UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""GameId"" = p_GameId
                ORDER BY ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByName = new()
    {
        Table = Table,
        Action = "GetByName",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByName"" (
                p_Name VARCHAR(128),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Name"" = p_Name
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
                p_Name VARCHAR(128),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                INSERT into ""{Table.TableName}"" (""GameId"", ""Name"")
                VALUES (p_GameId, p_Name)
                RETURNING ""Id"" INTO p_;
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
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR 
                SELECT *
                FROM ""{Table.TableName}""
                WHERE 
                    CAST(""Id"" AS TEST) ILIKE '%' || p_SearchTerm || '%'
                    OR ""Name"" ILIKE '%' || p_SearchTerm || '%';
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure SearchPaginated = new()
    {
        Table = Table,
        Action = "SearchPaginated",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_SearchPaginated"" (
                p_SearchTerm VARCHAR(256),
                p_Offset INT,
                p_PageSize INT,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT COUNT(*) OVER() AS ""TotalCount"", *
                FROM ""{Table.TableName}""
                WHERE 
                    CAST(""Id"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                    OR ""Name"" ILIKE '%' || p_SearchTerm || '%'
                ORDER BY ""Name"" ASC
                OFFSET p_Offset LIMIT p_PageSize;
            END;
            $$;"
    };
}