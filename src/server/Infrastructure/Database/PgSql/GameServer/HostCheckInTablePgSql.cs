using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class HostCheckInTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "HostCheckIns";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(HostCheckInTablePgSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 10,
        TableName = TableName,
        SqlStatement = $@"
            CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                ""Id"" UUID PRIMARY KEY,
                ""HostId"" UUID NOT NULL,
                ""SendTimestamp"" TIMESTAMP NOT NULL,
                ""ReceiveTimestamp"" TIMESTAMP NOT NULL,
                ""CpuUsage"" REAL NOT NULL,
                ""RamUsage"" REAL NOT NULL,
                ""Uptime"" REAL NOT NULL,
                ""NetworkOutBytes"" INTERGER NOT NULL,
                ""NetworkInBytes"" INTERGER NOT NULL
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
                FROM ""{Table.TableName}"" h;
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
                FROM ""{Table.TableName}""]
                ORDER BY ""Id"" DESC 
                OFFSET p_Offset LIMIT p_PageSize;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetAllAfter = new()
    {
        Table = Table,
        Action = "GetAllAfter",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAllAfter"" (
                IN p_AfterDate TIMESTAMP,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""ReceiveTimestamp"" > p_AfterDate
                ORDER BY ""ReceiveTimestamp"" DESC;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetAfterByHostId = new()
    {
        Table = Table,  
        Action = "GetAfterByHostId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAfterByHostId""
                IN p_Id UUID,
                IN p_AfterDate TIMESTAMP,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}"" h
                WHERE ""HostId"" = p_Id AND ""ReceiveTimestamp"" > p_AfterDate
                ORDER BY ""ReceiveTimestamp"" DESC;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetById = new()
    {
        Table = Table,
        Action = "GetById",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetById"" (
                IN p_Id int,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}"" h
                WHERE ""Id"" = p_Id
                ORDER BY ""Id"";
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
                FROM ""{Table.TableName}"" h
                WHERE ""HostId"" = p_HostId
                ORDER BY ""Id"";
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByHostIdLatest = new()
    {
        Table = Table,
        Action = "GetByHostIdLatest",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByHostIdLatest"" (
                IN p_HostId VARCHAR(256),
                IN p_Count INT,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}"" h
                WHERE ""HostId"" = p_HostId
                ORDER BY ""SendTimestamp"" DESC;
                LIMIT p_Count;
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
                IN p_SendTimestamp TIMESTAMP,
                IN p_ReceiveTimestamp TIMESTAMP,
                IN p_CpuUsage REAL,
                IN p_RamUsage REAL,
                IN p_Uptime REAL,
                IN p_NetworkOutBytes INT,
                IN p_NetworkInBytes INT,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                INSERT INTO ""{Table.TableName}"" (
                    ""HostId"", ""SendTimestamp"", ""ReceiveTimestamp"", 
                    ""CpuUsage"", ""RamUsage"", ""Uptime"", 
                    ""NetworkOutBytes"", ""NetworkInBytes""
                )
                VALUES (
                    p_HostId, p_SendTimestamp, p_ReceiveTimestamp, 
                    p_CpuUsage, p_RamUsage, p_Uptime, 
                    p_NetworkOutBytes, p_NetworkInBytes
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
                FROM ""{Table.TableName}"" h
                WHERE ""Id""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR ""HostId""::TEXT ILIKE '%' || p_SearchTerm || '%';
            END:
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
                    OR ""HostId"" ILIKE '%' || p_SearchTerm || '%'
                ORDER BY ""Id"" DESC 
                OFFSET p_Offset LIMIT p_PageSize;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure DeleteAllForHostId = new()
    {
        Table = Table,
        Action = "DeleteAllForHostId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_DeleteAllForHostId"" (
                IN p_HostId UUID
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                DELETE FROM ""{Table.TableName}""
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
                IN p_OlderThan TIMESTAMP
            )
            LANGUAGE plpgsql
            AS
            begin
                DELETE FROM ""{Table.TableName}""
                WHERE ""ReceiveTimestamp"" < p_OlderThan;
            END;
            $$;"
    };
}