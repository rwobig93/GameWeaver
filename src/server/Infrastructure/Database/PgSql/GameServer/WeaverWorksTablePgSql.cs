using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class WeaverWorksTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "WeaverWorks";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(WeaverWorksTablePgSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 10,
        TableName = TableName,
        SqlStatement = $@"
            CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                ""Id""  UUID PRIMARY KEY,
                ""HostId"" UUID NOT NULL,
                ""TargetType"" INT NOT NULL,
                ""Status"" BOOLEAN NOT NULL,
                ""WorkData"" BYTEA NOT NULL,
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
                IN p_Id INT
            )  
            LANGUAGE plpgsql
            AS $$
            BEGIN
                DELETE FROM ""{Table.TableName}""
                WHERE ""Id"" = p_Id;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure DeleteAllForHostId = new()
    {
        Table = Table,
        Action = "DeleteAllForHostId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_DeleteAllForHostId"" (
                IN p_HostId UUID,
            )
            LANGUAGE plpgsql
            AS $$
            begin
                DELETE
                FROM ""{Table.TableName}""
                WHERE ""HostId"" = p_HostId;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure DeleteOlderThan = new()
    {
        Table = Table,
        Action = "DeleteOlderThan",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_DeleteOlderThan"" (
                IN  p_OlderThan DATETIME2,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
                WHERE CreatedOn < @OlderThan;
            end"
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
            CREATE OR ALTER PROCEDURE ""sp{Table.TableName}_GetAllPaginated"" (
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
                IN p_Id INT,
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
                IN p_HostId UUID,
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
    
    public static readonly SqlStoredProcedure GetTenWaitingByHostId = new()
    {
        Table = Table,
        Action = "GetWaitingByHostId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetWaitingByHostId"" (
                IN p_HostId UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM dbo.[{Table.TableName}] w
                WHERE ""HostId"" = p_HostId AND ""Status"" = FALSE
                ORDER BY ""Id""
                LIMIT 10;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetAllWaitingByHostId = new()
    {
        Table = Table,
        Action = "GetAllWaitingByHostId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAllWaitingByHostId"" (
                IN p_HostId UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""HostId"" = p_HostId AND Status = FALSE
                ORDER BY ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByTargetType = new()
    {
        Table = Table,
        Action = "GetByTargetType",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByTargetType]
                IN p_TargetType INT,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""TargetType"" = p_TargetType
                ORDER BY w.Id;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByStatus = new()
    {
        Table = Table,
        Action = "GetByStatus",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByStatus"" (
                IN p_Status INT,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Status"" = p_Status
                ORDER BY ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetAllCreatedWithin = new()
    {
        Table = Table,
        Action = "GetAllCreatedWithin",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAllCreatedWithin"" (
                IN p_From DATETIME2,
                IN p_Until DATETIME2,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""CreatedOn"" > p_From AND ""CreatedOn"" < p_Until
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
                IN p_TargetType INT,
                IN p_Status INT,
                IN p_WorkData BYTEA,
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
                WITH INSERTED AS (
                    INSERT INTo ""{Table.TableName}"" (
                        ""HostId"", ""TargetType"", ""Status"", ""WorkData"", 
                        ""CreatedBy"", ""CreatedOn"", ""LastModifiedBy"", ""LastModifiedOn""
                    )
                    VALUES (
                    p_HostId, p_TargetType, p_Status, p_WorkData, 
                    p_CreatedBy, p_CreatedOn, p_LastModifiedBy, p_LastModifiedOn
                    )
                    RETURNING ""Id""
                )
                SELECT ""Id"" FROM INSERTED;
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
                    OR ""HostId""::TEXT ILIKE '%' || p_SearchTerm || '%';
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
                    OR ""HostId""::TEXT ILIKE '%' || p_SearchTerm || '%'
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
                IN p_Id INT,
                IN p_HostId UUID = null,
                IN p_TargetType INT = null,
                IN p_Status BOOLEAN = null,
                IN p_WorkData BYTEA = null,
                IN p_CreatedBy UUID = null,
                IN p_CreatedOn TIMESTAMP = null,
                IN p_LastModifiedBy UUID = null,
                IN p_LastModifiedOn TIMESTAMP = null
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE ""{Table.TableName}""
                SET 
                    ""HostId"" = COALESCE(p_HostId, ""HostId""),
                    ""TargetType"" = COALESCE(p_TargetType, ""TargetType""), 
                    ""Status"" = COALESCE(p_Status, ""Status""), 
                    ""WorkData"" = COALESCE(p_WorkData, ""WorkData""),
                    ""CreatedBy"" = COALESCE(p_CreatedBy, ""CreatedBy""), 
                    ""CreatedOn"" = COALESCE(p_CreatedOn, ""CreatedOn""), 
                    ""LastModifiedBy"" = COALESCE(p_LastModifiedBy, ""LastModifiedBy""),
                    ""LastModifiedOn"" = COALESCE(p_LastModifiedOn, ""LastModifiedOn"")
                WHERE ""Id"" = p_Id;
            END;
            $$;"
    };
}