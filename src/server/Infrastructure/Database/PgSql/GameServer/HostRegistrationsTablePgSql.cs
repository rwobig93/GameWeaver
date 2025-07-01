using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class HostRegistrationsTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "HostRegistrations";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(HostRegistrationsTablePgSql).GetDbScriptsFromClass();

    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 10,
        TableName = TableName,
        SqlStatement = $@"
            CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                ""Id"" UUID PRIMARY KEY,
                ""HostId"" UUID NOT NULL,
                ""Description"" VARCHAR(2048) NOT NULL,
                ""Active"" BOOLEAN NOT NULL,
                ""Key"" VARCHAR(256) NOT NULL,
                ""ActivationDate"" TIMESTAMP NULL,
                ""ActivationPublicIp"" VARCHAR(128) NULL,
                ""CreatedBy"" UUID NOT NULL,
                ""CreatedOn"" TIMESTAMP NOT NULL,
                ""LastModifiedBy"" UUID NULL,
                ""LastModifiedOn"" TIMESTAMP NULL
            );"
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
                OPEN P_ FOR
                SELECT COUNT(*) OVER() AS TotalCount, *
                FROM ""{Table.TableName}""
                ORDER BY ""Id"" DESC 
                OFFSET p_Offset LIMIT p_PageSize;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetAllActive = new()
    {
        Table = Table,
        Action = "GetAllActive",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAllActive"" (
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Active"" = TRUE;
            END;
            $$"
    };
    
    public static readonly SqlStoredProcedure GetAllInActive = new()
    {
        Table = Table,
        Action = "GetAllInActive",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAllInActive"" (
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Active"" = FALSE;
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
    
    public static readonly SqlStoredProcedure GetByHostId = new()
    {
        Table = Table,
        Action = "GetByHostId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByHostId"" (
                IN p_HostId VARCHAR(256),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""HostId"" = p_HostId
                ORDER BY ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByHostIdAndKey = new()
    {
        Table = Table,
        Action = "GetByHostIdAndKey",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByHostIdAndKey""(
                IN p_HostId VARCHAR(256),
                IN p_Key VARCHAR(256),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""HostId"" = p_HostId 
                AND ""Key"" = p_Key
                AND ""Active"" = TRUE
                ORDER BY ""Id""
                LIMIT 1;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetActiveByDescription = new()
    {
        Table = Table,
        Action = "GetActiveByDescription",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetActiveByDescription"" (
                IN p_Description VARCHAR(256),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE h.Active = 1 
                AND ""Description"" = p_Description
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
                IN p_HostId UUID,
                IN p_Description VARCHAR(2048),
                IN p_Active BOOLEAN,
                IN p_Key VARCHAR(256),
                IN p_ActivationDate TIMESTAMP,
                IN p_ActivationPublicIp VARCHAR(128),
                IN p_CreatedBy UUID,
                IN p_CreatedOn TIMESTAMP,
                IN p_LastModifiedBy UUID,
                IN p_LastModifiedOn TIMESTAMP,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                INSERT INTO ""{{Table.TableName}}"" (
                    ""HostId"", ""Description"", ""Active"", ""Key"", ""ActivationDate"",
                    ""ActivationPublicIp"", ""CreatedBy"", ""CreatedOn"", ""LastModifiedBy"", ""LastModifiedOn""
                )
                VALUES (
                    p_HostId, p_Description, p_Active, p_Key, p_ActivationDate,
                    p_ActivationPublicIp, p_CreatedBy, p_CreatedOn, p_LastModifiedBy, p_LastModifiedOn
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
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Id""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""HostId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""Key"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""Description"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""ActivationPublicIp"" ILIKE '%' || p_SearchTerm || '%';
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
                    OR ""HostId"":: ILIKE '%' || p_SearchTerm || '%'
                    OR ""Key"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""Description"" ILIKE '%' || p_SearchTerm || '%'
                    OR ""ActivationPublicIp"" ILIKE '%' || p_SearchTerm || '%'
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
                IN p_HostId UUID DEFAULT NULL,
                IN p_Description VARCHAR(2048) DEFAULT NULL,
                IN p_Active BOOLEAN DEFAULT NULL,
                IN p_Key VARCHAR(256) DEFAULT NULL,
                IN p_ActivationDate TIMESTAMP DEFAULT NULL,
                IN p_ActivationPublicIp VARCHAR(128) DEFAULT NULL,
                IN p_CreatedBy UUID DEFAULT NULL,
                IN p_CreatedOn TIMESTAMP DEFAULT NULL,
                IN p_LastModifiedBy UUID DEFAULT NULL,
                IN p_LastModifiedOn TIMESTAMP DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE ""{Table.TableName}""
                SET 
                    ""HostId"" = COALESCE(p_HostId, ""HostId""), 
                    ""Description"" = COALESCE(p_Description, ""Description""), 
                    ""Active"" = COALESCE(p_Active, ""Active""), 
                    ""Key"" = COALESCE(p_Key, ""Key""),
                    ""ActivationDate"" = COALESCE(p_ActivationDate, ""ActivationDate""), 
                    ""ActivationPublicIp"" = COALESCE(p_ActivationPublicIp, ""ActivationPublicIp""),
                    ""CreatedBy"" = COALESCE(p_CreatedBy, ""CreatedBy""), 
                    ""CreatedOn"" = COALESCE(p_CreatedOn, ""CreatedOn""), 
                    ""LastModifiedBy"" = COALESCE(p_LastModifiedBy, ""LastModifiedBy""),
                    ""LastModifiedOn"" = COALESCE(p_LastModifiedOn, ""LastModifiedOn"")
                WHERE ""Id"" = p_Id;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure DeleteOlderThan = new()
    {
        Table = Table,
        Action = "DeleteOlderThan",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_DeleteOlderThan"" (
                IN p_OlderThan TIMESTAMP,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                DELETE FROM ""{Table.TableName}""
                WHERE ""Active"" = FALSE
                    AND ""CreatedOn"" < p_OlderThan;
            END;
            $$;"
    };
}