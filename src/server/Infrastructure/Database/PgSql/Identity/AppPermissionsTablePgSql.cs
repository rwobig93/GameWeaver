using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.Identity;

public class AppPermissionsTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "AppPermissions";
    
    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(AppPermissionsTablePgSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 3,
        TableName = TableName,
        SqlStatement = $@"
            CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                ""Id"" UUID PRIMARY KEY,
                ""RoleId"" UUID NULL,
                ""UserId"" UUID NULL,
                ""ClaimType"" VARCHAR(256) NULL,
                ""ClaimValue"" VARCHAR(1024) NULL,
                ""Name"" VARCHAR(256) NOT NULL,
                ""Group"" VARCHAR(256) NOT NULL,
                ""Access"" VARCHAR(256) NOT NULL,
                ""Description"" VARCHAR(4000) NOT NULL,
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
    
    public static readonly SqlStoredProcedure DeleteForUser = new()
    {
        Table = Table,
        Action = "DeleteForUser",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_DeleteForUser"" (
                IN p_UserId UUID
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                DELETE FROM ""{Table.TableName}""
                WHERE ""UserId"" = p_UserId;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure DeleteForRole = new()
    {
        Table = Table,
        Action = "DeleteForRole",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_DeleteForRole"" (
                IN p_RoleId UUID
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                DELETE FROM ""{Table.TableName}""
                WHERE ""RoleId"" = p_RoleId;
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
                SELECT COUNT(*) OVER() AS ""TotalCount"", *
                FROM ""{Table.TableName}""
                ORDER BY ""Id"" DESC 
                OFFSET p_Offset LIMIT p_PageSize;
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetAllUsersByClaimValue = new()
    {
        Table = Table,
        Action = "GetAllUsersByClaimValue",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAllUsersByClaimValue"" (
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT u.*, s.""PasswordHash"", s.""PasswordSalt"", s.""TwoFactorEnabled"", s.""TwoFactorKey"", s.""AuthState"", s.AuthStateTimestamp,
                        s.""BadPasswordAttempts"", s.""LastBadPassword"", s.""LastFullLogin""
                FROM ""{Table.TableName}"" p
                JOIN ""{AppUsersTablePgSql.Table.TableName}"" u ON u.""Id"" = p.""UserId""
                JOIN ""{AppUserSecurityAttributesTablePgSql.Table.TableName}"" s ON u.""Id"" = s.""OwnerId""
                WHERE u.""IsDeleted"" = FALSE;
            END;
            $$;"
    };

    public static readonly SqlStoredProcedure GetAllRolesByClaimValue = new()
    {
        Table = Table,
        Action = "GetAllRolesByClaimValue",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetAllRolesByClaimValue"" (
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT r.*
                FROM ""{Table.TableName}"" p
                JOIN ""{AppRolesTablePgSql.Table.TableName}"" r ON r.""Id"" = p.""RoleId"";
            end"
    };
    
    public static readonly SqlStoredProcedure GetById = new()
    {
        Table = Table,
        Action = "GetById",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetById""
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
    
    public static readonly SqlStoredProcedure GetByName = new()
    {
        Table = Table,
        Action = "GetByName",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByName"" (
                IN p_Name VARCHAR(256),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Name"" = p_Name
                ORDER BY ""Id""
                LIMIT 1;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByGroup = new()
    {
        Table = Table,
        Action = "GetByGroup",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByGroup"" (
                IN p_Group VARCHAR(256),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                SELECT p.*
                FROM ""{Table.TableName}""
                WHERE p.""Group"" = p_Group;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByAccess = new()
    {
        Table = Table,
        Action = "GetByAccess",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByAccess"" (
                IN p_Access VARCHAR(256),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT p.*
                FROM ""{Table.TableName}"" p
                WHERE p.""Access"" = p_Access;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByClaimValue = new()
    {
        Table = Table,
        Action = "GetByClaimValue",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByClaimValue"" (
                IN p_ClaimValue VARCHAR(1024),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT p.*
                FROM ""{Table.TableName}""
                WHERE p.""ClaimValue"" = p_ClaimValue;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByRoleId = new()
    {
        Table = Table,
        Action = "GetByRoleId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE ""sp{Table.TableName}_GetByRoleId"" (
                IN p_RoleId UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT p.*
                FROM ""{Table.TableName}"" p
                WHERE p.""RoleId"" = p_RoleId;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByRoleIdAndValue = new()
    {
        Table = Table,
        Action = "GetByRoleIdAndValue",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByRoleIdAndValue"" (
                IN p_RoleId UUID,
                IN p_ClaimValue VARCHAR(1024),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT p.*
                FROM ""{Table.TableName}"" p
                WHERE p.""RoleId"" = p_RoleId AND p.""ClaimValue"" = p_ClaimValue;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByUserId = new()
    {
        Table = Table,
        Action = "GetByUserId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByUserId"" (
                IN p_UserId UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT p.*
                FROM ""{Table.TableName}"" p
                WHERE p.""UserId"" = p_UserId;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByUserIdAndValue = new()
    {
        Table = Table,
        Action = "GetByUserIdAndValue",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByUserIdAndValue"" (
                IN p_UserId UUID,
                IN p_ClaimValue VARCHAR(1024),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT p.*
                FROM ""{Table.TableName}"" p
                WHERE p.""UserId"" = p_UserId AND p.""ClaimValue"" = p_ClaimValue;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Insert"" (
                IN p_RoleId UUID,
                IN p_UserId UUID,
                IN p_ClaimType VARCHAR(256),
                IN p_ClaimValue VARCHAR(1024),
                IN p_Name VARCHAR(256),
                IN p_Group VARCHAR(256),
                IN p_Access VARCHAR(256),
                IN p_Description VARCHAR(4000),
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
                    INSERT into ""{Table.TableName}"" (
                        ""RoleId"", ""UserId"", ""Name"", ""Group"", 
                        ""Access"", ""ClaimType"", ""ClaimValue"", ""Description"", 
                        ""CreatedBy"", ""CreatedOn"", ""LastModifiedBy"", ""LastModifiedOn""
                    )
                    VALUES (
                        p_RoleId, p_UserId, p_Name, p_Group, 
                        p_Access, p_ClaimType, p_ClaimValue, p_Description, 
                        p_CreatedBy, p_CreatedOn, p_LastModifiedBy, p_LastModifiedOn
                    )
                    RETURNING ""Id""
                )
                SELECT * FROM INSERTED;
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
                SELECT p.*
                FROM ""{Table.TableName}"" p
                WHERE p.""Description"" ILIKE '%' || p_SearchTerm || '%'
                    OR p.""RoleId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR p.""UserId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR p.""ClaimValue"" ILIKE '%' || p_SearchTerm || '%';
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
                SELECT COUNT(*) OVER() AS ""TotalCount"", p.*
                FROM ""{Table.TableName}"" p
                WHERE p.""Description"" ILIKE '%' || p_SearchTerm || '%'
                    OR p.""RoleId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR p.""UserId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    OR p.""ClaimValue"" ILIKE '%' || p_SearchTerm || '%'
                ORDER BY p.""Id"" DESC 
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
                IN p_RoleId UUID DEFAULT NULL,
                IN p_UserId UUID DEFAULT NULL,
                IN p_ClaimType VARCHAR(256) DEFAULT NULL,
                IN p_ClaimValue VARCHAR(1024) DEFAULT NULL,
                IN p_Name VARCHAR(256) DEFAULT NULL,
                IN p_Group VARCHAR(256) DEFAULT NULL,
                IN p_Access VARCHAR(256) DEFAULT NULL,
                IN p_Description VARCHAR(4000) DEFAULT NULL,
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
                    ""RoleId"" = COALESCE(p_RoleId, ""RoleId""), 
                    ""UserID"" = COALESCE(p_UserId, ""UserId""), 
                    ""ClaimType"" = COALESCE(p_ClaimType, ""ClaimType""),
                    ""ClaimValue"" = COALESCE(p_ClaimValue, ""ClaimValue""), 
                    ""Name"" = COALESCE(p_Name, ""Name""), 
                    ""Group"" = COALESCE(p_Group, ""Group""),
                    ""Access"" = COALESCE(p_Access, ""Access""), 
                    ""Description"" = COALESCE(p_Description, ""Description""),
                    ""CreatedBy"" = COALESCE(p_CreatedBy, ""CreatedBy""), 
                    ""CreatedOn"" = COALESCE(p_CreatedOn, ""CreatedOn""),
                    ""LastModifiedBy"" = COALESCE(p_LastModifiedBy, ""LastModifiedBy""), 
                    ""LastModifiedOn"" = COALESCE(p_LastModifiedOn, ""LastModifiedOn"")
                WHERE ""Id"" = p_Id;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetDynamicByTypeAndName = new()
    {
        Table = Table,
        Action = "GetDynamicByTypeAndName",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetDynamicByTypeAndName"" (
                IN p_Group VARCHAR(256),
                IN p_Name VARCHAR(256),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT p.*
                FROM ""{Table.TableName}"" p
                WHERE p.""ClaimType"" = 'DynamicPermission'
                    AND p.""Group"" = p_Group
                    AND p.""Name"" = p_Name;
            END;
            $$;"
    };
}