using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class HostsTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "Hosts";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(HostsTablePgSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 9,
        TableName = TableName,
        SqlStatement = $@"
            CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                ""Id"" UUID PRIMARY KEY,
                ""OwnerId"" UUID NOT NULL,
                ""PasswordHash"" VARCHAR(256) NOT NULL,
                ""PasswordSalt"" VARCHAR(256) NOT NULL,
                ""Hostname"" VARCHAR(256) NULL,
                ""FriendlyName"" VARCHAR(256) NULL,
                ""Description"" VARCHAR(2048) NULL,
                ""PrivateIp"" VARCHAR(128) NULL,
                ""PublicIp"" VARCHAR(128) NULL,
                ""CurrentState"" INT NOT NULL,
                ""Os"" INT NOT NULL,
                ""OsName"" VARCHAR(1024) NULL,
                ""OsVersion"" VARCHAR(1024) NULL,
                ""AllowedPorts"" VARBINARY(4096) NULL,
                ""Cpus"" VARBINARY(4096) NULL,
                ""Motherboards"" VARBINARY(4096) NULL,
                ""Storage"" VARBINARY(4096) NULL,
                ""NetworkInterfaces"" VARBINARY(4096) NULL,
                ""RamModules"" VARBINARY(4096) NULL,
                ""Gpus"" VARBINARY(4096) NULL,
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
                IN p_Id UUID,
                IN p_DeletedBy UUID,
                IN p_DeletedOn TIMESTAMP,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
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
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""IsDeleted"" = FALSE
                ORDER BY ""FriendlyName"";
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
                WHERE ""IsDeleted"" = FALSE AND ""CurrentState"" != 1
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
            CREATE OR REPLACE PROCEDURE [dbo].[sp{Table.TableName}_GetById]
                IN p_Id UUID,
                INOUT p_ REFCURSOR
            AS
            begin
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Id"" = p_Id
                ORDER BY ""Id""
                LIMIT 1;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByHostname = new()
    {
        Table = Table,
        Action = "GetByHostname",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByHostname"" (
                IN p_Hostname VARCHAR(256),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Hostname"" = p_Hostname AND ""IsDeleted"" = FALSE
                ORDER BY h.Id
                LIMIT 1;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE ""sp{Table.TableName}_Insert"" (
                IN p_OwnerId UUID,
                IN p_PasswordHash VARCHAR(256),
                IN p_PasswordSalt VARCHAR(256),
                IN p_Hostname VARCHAR(256),
                IN p_FriendlyName VARCHAR(256),
                IN p_Description VARCHAR(2048),
                IN p_PrivateIp VARCHAR(128),
                IN p_PublicIp VARCHAR(128),
                IN p_CurrentState INT,
                IN p_Os INT,
                IN p_OsName VARCHAR(1024),
                IN p_OsVersion VARCHAR(1024),
                IN p_AllowedPorts BYTEA(4096),
                IN p_Cpus BYTEA(4096),
                IN p_Motherboards BYTEA(4096),
                IN p_Storage BYTEA(4096),
                IN p_NetworkInterfaces BYTEA(4096),
                IN p_RamModules BYTEA(4096),
                IN p_Gpus BYTEA(4096),
                IN p_CreatedBy UUID,
                IN p_CreatedOn TIMESTAMP,
                IN p_LastModifiedBy UUID,
                IN p_LastModifiedOn TIMESTAMP,
                IN p_IsDeleted BOOLEAN,
                IN p_DeletedOn TIMESTAMP,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                INSERT INTO ""{{Table.TableName}}"" (
                    ""OwnerId"", ""PasswordHash"", ""PasswordSalt"", ""Hostname"", ""FriendlyName"", ""Description"",
                    ""PrivateIp"", ""PublicIp"", ""CurrentState"", ""Os"", ""OsName"", ""OsVersion"",
                    ""AllowedPorts"", ""Cpus"", ""Motherboards"", ""Storage"", ""NetworkInterfaces"", ""RamModules"", ""Gpus"",
                    ""CreatedBy"", ""CreatedOn"", ""LastModifiedBy"", ""LastModifiedOn"", ""IsDeleted"", ""DeletedOn""
                )
                VALUES (
                    p_OwnerId, p_PasswordHash, p_PasswordSalt, p_Hostname, p_FriendlyName, p_Description,
                    p_PrivateIp, p_PublicIp, p_CurrentState, p_Os, p_OsName, p_OsVersion,
                    p_AllowedPorts, p_Cpus, p_Motherboards, p_Storage, p_NetworkInterfaces, p_RamModules, p_Gpus,
                    p_CreatedBy, p_CreatedOn, p_LastModifiedBy, p_LastModifiedOn, p_IsDeleted, p_DeletedOn
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
                WHERE ""IsDeleted"" = FALSE AND ""CurrentState"" != 1
                    AND (
                        ""Id""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""Hostname"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""FriendlyName"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""Description"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""PrivateIp"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""PublicIp"" ILIKE '%' || p_SearchTerm || '%'
                    )
                ORDER BY ""Id"";
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
                SELECT COUNT(*) OVER() AS ""TotalCount"", *
                FROM ""{Table.TableName}""
                WHERE ""IsDeleted"" = FALSE AND ""CurrentState"" != 1
                    AND (
                        ""Id""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""Hostname"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""FriendlyName"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""Description"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""PrivateIp"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""PublicIp"" ILIKE '%' || p_SearchTerm || '%'
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
                IN p_Id UUID,
                IN p_OwnerId UUID DEFAULT NULL,
                IN p_PasswordHash VARCHAR(256) DEFAULT NULL,
                IN p_PasswordSalt VARCHAR(256) DEFAULT NULL,
                IN p_Hostname VARCHAR(256) DEFAULT NULL,
                IN p_FriendlyName VARCHAR(256) DEFAULT NULL,
                IN p_Description VARCHAR(2048) DEFAULT NULL,
                IN p_PrivateIp VARCHAR(128) DEFAULT NULL,
                IN p_PublicIp VARCHAR(128) DEFAULT NULL,
                IN p_CurrentState INT DEFAULT NULL,
                IN p_Os INT DEFAULT NULL,
                IN p_OsName VARCHAR(1024) DEFAULT NULL,
                IN p_OsVersion VARCHAR(1024) DEFAULT NULL,
                IN p_AllowedPorts BYTEA (4096) DEFAULT NULL,
                IN p_Cpus BYTEA (4096) DEFAULT NULL,
                IN p_Motherboards BYTEA (4096) DEFAULT NULL,
                IN p_Storage BYTEA (4096) DEFAULT NULL,
                IN p_NetworkInterfaces BYTEA (4096) DEFAULT NULL,
                IN p_RamModules BYTEA (4096) DEFAULT NULL,
                IN p_Gpus BYTEA (4096) DEFAULT NULL,
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
                    ""OwnerId"" = COALESCE(p_OwnerId, ""OwnerId""), 
                    ""PasswordHash"" = COALESCE(p_PasswordHash, ""PasswordHash""), 
                    ""PasswordSalt"" = COALESCE(p_PasswordSalt, ""PasswordSalt""),
                    ""Hostname"" = COALESCE(p_Hostname, ""Hostname""), 
                    ""FriendlyName"" = COALESCE(p_FriendlyName, ""FriendlyName""), 
                    ""Description"" = COALESCE(p_Description, ""Description""),
                    ""PrivateIp"" = COALESCE(p_PrivateIp, ""PrivateIp""), 
                    ""PublicIp"" = COALESCE(p_PublicIp, ""PublicIp""), 
                    ""CurrentState"" = COALESCE(p_CurrentState, ""CurrentState""),
                    ""Os"" = COALESCE(p_Os, ""Os""), 
                    ""OsName"" = COALESCE(p_OsName, ""OsName""), OsVersion = COALESCE(@OsVersion, OsVersion),
                    ""AllowedPorts"" = COALESCE(p_AllowedPorts, ""AllowedPorts""), 
                    ""Cpus"" = COALESCE(p_Cpus, ""Cpus""), Motherboards = COALESCE(@Motherboards, Motherboards),
                    ""Storage"" = COALESCE(p_Storage, ""Storage""), 
                    ""NetworkInterfaces"" = COALESCE(p_NetworkInterfaces, ""NetworkInterfaces""), 
                    ""RamModules"" = COALESCE(p_RamModules, ""RamModules""),
                    ""Gpus"" = COALESCE(p_Gpus, ""Gpus""), 
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
    
    public static readonly SqlStoredProcedure DeleteUnregisteredOlderThan = new()
    {
        Table = Table,
        Action = "DeleteUnregisteredOlderThan",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_DeleteUnregisteredOlderThan"" (
                IN p_OlderThan TIMESTAMP
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                DELETE FROM ""{Table.TableName}""
                WHERE ""CurrentState"" = 1
                    AND ""CreatedOn"" < p_OlderThan;
            END;
            $$;"
    };
}