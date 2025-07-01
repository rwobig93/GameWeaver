using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class ConfigurationItemsTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "ConfigurationItems";
    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(ConfigurationItemsTablePgSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 10,
        TableName = TableName,
        SqlStatement = $@"
            CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                ""Id"" UUID PRIMARY KEY,
                ""LocalResourceId"" UUID NOT NULL,
                ""DuplicateKey"" BIT NOT NULL,
                ""Path"" VARCHAR(128) NOT NULL,
                ""Category"" VARCHAR(128) NOT NULL,
                ""Key"" VARCHAR(128) NOT NULL,
                ""Value"" VARCHAR(128) NOT NULL,
                ""FriendlyName"" VARCHAR(128) NULL,
                
                FOREIGN KEY (""LocalResourceId"") 
                    REFERENCES ""{LocalResourcesTablePgSql.Table.TableName}""(""Id"") 
                    ON DELETE CASCADE
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
                SELECT h.*
                FROM ""{Table.TableName}"";
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
                SELECT COUNT(*) OVER() AS ""TotalCount"", h.*
                FROM ""{Table.TableName}""
                ORDER BY ""Id"" DESC
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
                SELECT h.*
                FROM ""{Table.TableName}""
                WHERE ""Id"" = p_Id
                ORDER BY ""Id""
                LIMIT 1;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByLocalResourceId = new()
    {
        Table = Table,
        Action = "GetByLocalResourceId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByLocalResourceId"" (
                IN p_LocalResourceId UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT h.*
                FROM ""{Table.TableName}""
                WHERE ""LocalResourceId"" = p_LocalResourceId
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
                IN p_LocalResourceId UUID,
                IN p_DuplicateKey BIT,
                IN p_Path VARCHAR(128),
                IN p_Category VARCHAR(128),
                IN p_Key VARCHAR(128),
                IN p_Value VARCHAR(128),
                IN p_FriendlyName VARCHAR(128),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                INSERT into ""{Table.TableName}"" (
                    ""LocalResourceId"", ""DuplicateKey"", ""Path"", ""Category"", ""Key"", ""Value"", ""FriendlyName""
                )
                VALUES (
                    p_LocalResourceId, p_DuplicateKey, p_Path, p_Category, p_Key, p_Value, p_FriendlyName
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
                IN p_SearchTerm VARCHAR(256),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT h.*
                FROM ""{Table.TableName}""
                WHERE CAST(""Id"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                    OR CAST(""LocalResourceId"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                    OR ""Path"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""Category"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""Key"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""Value"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""FriendlyName"" ILIKE '%' || p_SearchTerm || '%';
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
                SELECT COUNT(*) OVER() AS ""TotalCount"", h.*
                FROM ""{Table.TableName}""
                WHERE CAST(""Id"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                    OR CAST(""LocalResourceId"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                    OR ""Path"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""Category"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""Key"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""Value"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""FriendlyName"" ILIKE '%' || p_SearchTerm || '%'
                ORDER BY ""Id"" DESC 
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
                IN p_Id UUID,
                IN p_LocalResourceId UUID DEFAULT NULL,
                IN p_DuplicateKey BIT DEFAULT NULL,
                IN p_Path VARCHAR(128) DEFAULT NULL,
                IN p_Category VARCHAR(128) DEFAULT NULL,
                IN p_Key VARCHAR(128) DEFAULT NULL,
                IN p_Value VARCHAR(128) DEFAULT NULL,
                IN p_FriendlyName VARCHAR(128) DEFAULT NULL,
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE ""{Table.TableName}""
                SET 
                    ""LocalResourceId"" = COALESCE(p_LocalResourceId, ""LocalResourceId""), 
                    ""DuplicateKey"" = COALESCE(p_DuplicateKey, ""DuplicateKey""), 
                    ""Path"" = COALESCE(p_Path, ""Path""),
                    ""Category"" = COALESCE(p_Category, ""Category""), 
                    ""Key"" = COALESCE(p_Key, ""Key""), 
                    ""Value"" = COALESCE(p_Value, ""Value""), 
                    ""FriendlyName"" = COALESCE(p_FriendlyName, ""FriendlyName"")
                WHERE ""Id"" = p_Id;
            END;
            $$;"
    };
    
}