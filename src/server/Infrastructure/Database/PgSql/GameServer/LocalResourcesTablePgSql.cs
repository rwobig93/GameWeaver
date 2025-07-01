using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class LocalResourcesTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "LocalResources";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(LocalResourcesTablePgSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 9,
        TableName = TableName,
        SqlStatement = $@"
            CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                ""Id"" UUID PRIMARY KEY,
                ""GameProfileId"" UUID NOT NULL,
                ""Name"" VARCHAR(128) NOT NULL,
                ""PathWindows"" VARCHAR(128) NOT NULL,
                ""PathLinux"" VARCHAR(128) NOT NULL,
                ""PathMac"" VARCHAR(128) NOT NULL,
                ""Startup"" BIT NOT NULL,
                ""StartupPriority"" INT NOT NULL,
                ""Type"" INT NOT NULL,
                ""ContentType"" INT NOT NULL,
                ""Args"" VARCHAR(128) NOT NULL,
                ""LoadExisting"" BIT NOT NULL,
                ""CreatedBy"" UUID NOT NULL,
                ""CreatedOn"" TIMESTAMP NOT NULL,
                ""LastModifiedBy"" UUID NULL,
                ""LastModifiedOn"" TIMESTAMP NULL
            );"
    };
    
    public static readonly SqlStoredProcedure Delete = new()
    {
        Table = Table,
        Action = "Delete",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Delete"" (
                IN p_Id UUID,
                INOUT p_ REFCURSOR
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
                IN p_Id UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Id"" = p_Id
                ORDER BY ""Id""
                LIMIT 1;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByGameProfileId = new()
    {
        Table = Table,
        Action = "GetByGameProfileId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByGameProfileId"" (
                IN p_GameProfileId UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""GameProfileId"" = p_GameProfileId
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
                IN p_Id UUID,
                IN p_GameProfileId UUID,
                IN p_Name VARCHAR(128),
                IN p_PathWindows VARCHAR(128),
                IN p_PathLinux VARCHAR(128),
                IN p_PathMac VARCHAR(128),
                IN p_Startup BIT,
                IN p_StartupPriority INT,
                IN p_Type INT,
                IN p_ContentType INT,
                IN p_Args VARCHAR(128),
                IN p_LoadExisting INT,
                IN p_CreatedBy UUID,
                IN p_CreatedOn TIMESTAMP,
                IN p_LastModifiedBy UUID,
                IN p_LastModifiedOn TIMESTAMP,
                OUT p_ UUID
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                INSERT into ""{Table.TableName}"" (
                    ""Id"","" GameProfileId"", ""Name"", ""PathWindows"", ""PathLinux"", ""PathMac"", ""Startup"", ""StartupPriority"", ""Type"", ""ContentType"", ""Args"", ""LoadExisting"",
                    ""CreatedBy"", ""CreatedOn"", ""LastModifiedBy"", ""LastModifiedOn""
                    )
                VALUES (
                    p_Id, p_GameProfileId, p_Name, p_PathWindows, p_PathLinux, p_PathMac, p_Startup, p_StartupPriority, p_Type, p_ContentType, p_Args, p_LoadExisting, p_CreatedBy,
                    p_CreatedOn, p_LastModifiedBy, p_LastModifiedOn
                )
                RETURNING ""ID"" INTO p_;
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
                WHERE 
                    CAST(""Id"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                    OR CAST(""GameProfileId"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                    OR ""Name"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""PathWindows"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""PathLinux"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""PathMac"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""Args"" ILIKE '%' || p_SearchTerm || '%';
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
                WHERE 
                    CAST(""Id"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                    OR CAST(""GameProfileId"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                    OR ""Name"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""PathWindows"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""PathLinux"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""PathMac"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""Args"" ILIKE '%' || p_SearchTerm || '%'
                ORDER BY ""Name"" ASC 
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
                IN p_GameProfileId UUID DEFAULT NULL,
                IN p_Name VARCHAR(128) DEFAULT NULL,
                IN p_PathWindows VARCHAR(128) DEFAULT NULL,
                IN p_PathLinux VARCHAR(128) DEFAULT NULL,
                IN p_PathMac VARCHAR(128) DEFAULT NULL,
                IN p_Startup BIT DEFAULT NULL,
                IN p_StartupPriority INT DEFAULT NULL,
                IN p_Type INT DEFAULT NULL,
                IN p_ContentType INT DEFAULT NULL,
                IN p_Args VARCHAR(128) DEFAULT NULL,
                IN p_LoadExisting INT DEFAULT NULL,
                IN p_LastModifiedBy UUID DEFAULT NULL,
                IN p_LastModifiedOn TIMESTAMP DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE ""{Table.TableName}""
                SET ""GameProfileId"" = COALESCE(p_GameProfileId, ""GameProfileId""), 
                    ""Name"" = COALESCE(p_Name, ""Name""),
                    ""PathWindows"" = COALESCE(p_PathWindows, ""PathWindows""), 
                    ""PathLinux"" = COALESCE(p_PathLinux, ""PathLinux""), 
                    ""PathMac"" = COALESCE(p_PathMac, ""PathMac""),
                    ""Startup"" = COALESCE(p_Startup, ""Startup""), 
                    ""StartupPriority"" = COALESCE(p_StartupPriority, ""StartupPriority""), 
                    ""Type"" = COALESCE(p_Type, ""Type""),
                    ""ContentType"" = COALESCE(p_ContentType, ""ContentType""), 
                    ""Args"" = COALESCE(p_Args, ""Args""), 
                    ""LoadExisting"" = COALESCE(p_LoadExisting, ""LoadExisting""),
                    ""LastModifiedBy"" = COALESCE(p_LastModifiedBy, ""LastModifiedBy""), 
                    ""LastModifiedOn"" = COALESCE(p_LastModifiedOn, ""LastModifiedOn"")
                WHERE ""Id"" = p_Id;
            END;
            $$;"
    };
}
