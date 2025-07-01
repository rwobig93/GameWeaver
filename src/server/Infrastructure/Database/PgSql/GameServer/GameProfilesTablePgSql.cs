using Application.Database;
using Application.Database.PgSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class GameProfilesTablePgSql : IPgSqlEnforcedEntity
{
    private const string TableName = "GameProfiles";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(GameProfilesTablePgSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 6,
        TableName = TableName,
        SqlStatement = $@"
        CREATE TABLE IF NOT EXISTS ""{TableName}"" (
            ""Id"" UUID PRIMARY KEY,
            ""FriendlyName"" VARCHAR(128) NOT NULL,
            ""OwnerId"" UUID NOT NULL,
            ""GameId"" UUID NOT NULL,
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
                IN p_DeletedOn TIMESTAMP
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
                SELECT h.*
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
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetById"" (
                IN p_Id UUID,
                INOUT p_ REFCURSOR,     
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
    
    public static readonly SqlStoredProcedure GetByOwnerId = new()
    {
        Table = Table,
        Action = "GetByOwnerId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByOwnerId"" (
                IN p_OwnerId UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                SELECT h.*
                FROM ""{Table.TableName}""
                WHERE ""OwnerId"" = p_OwnerId AND ""IsDeleted"" = FALSE
                ORDER BY g.Id;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByGameId = new()
    {
        Table = Table,
        Action = "GetByGameId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByGameId"" (
                IN p_GameId UUID,
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT h.*
                FROM ""{Table.TableName}""
                WHERE ""GameId"" = p_GameId AND ""IsDeleted"" = FALSE
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
                IN p_FriendlyName VARCHAR(128),
                INOUT p_ REFCURSOR
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT h.*
                FROM ""{Table.TableName}""
                WHERE ""FriendlyName"" = p_FriendlyName AND ""IsDeleted"" = FALSE
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
                IN p_FriendlyName VARCHAR(128),
                IN p_OwnerId UUID,
                IN p_GameId UUID,
                IN p_CreatedBy UUID,
                IN p_CreatedOn TIMESTAMP,
                IN p_LastModifiedBy UUID,
                IN p_LastModifiedOn TIMESTAMP,
                IN p_IsDeleted BOOLEAN,
                IN p_DeletedOn TIMESTAMP,
                OUT p_Id UUID
            )
            LANGAUGE plpgsql
            AS $$
            BEGIN
                INSERT into ""{Table.TableName}"" (
                    ""FriendlyName"", ""OwnerId"", ""GameId"", ""CreatedBy"", ""CreatedOn"", ""LastModifiedBy"", ""LastModifiedOn"", ""IsDeleted"", ""DeletedOn""
                )
                VALUES (
                    p_FriendlyName, p_OwnerId, p_GameId, p_CreatedBy, p_CreatedOn, 
                    p_LastModifiedBy, p_LastModifiedOn, p_IsDeleted, p_DeletedOn
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
                WHERE ""IsDeleted"" = FALSE
                    AND (
                        CAST(""Id"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                        OR CAST(""OwnerId"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                        OR CAST(""GameId"" AS TEXT) ILIKE '%' || p_SearchTerm || '%'
                        OR ""FriendlyName"" ILIKE '%' || p_SearchTerm || '%'
                    )
                ORDER BY ""Id"";
            end"
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
                SELECT COUNT(*) OVER() AS ""TotalCount"", h.*
                FROM ""{Table.TableName}""
                WHERE ""IsDeleted"" = FALSE
                    AND (
                       ""Id"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""FriendlyName"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""OwnerId"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""GameId"" ILIKE '%' || p_SearchTerm || '%'
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
                IN p_FriendlyName VARCHAR(128) = null,
                IN p_OwnerId UUID = null,
                IN p_GameId UUID = null,
                IN p_CreatedBy UUID = null,
                IN p_CreatedOn TIMESTAMP = null,
                IN p_LastModifiedBy UUID = null,
                IN p_LastModifiedOn TIMESTAMP = null,
                IN p_IsDeleted BOOLEAN = null,
                IN p_DeletedOn TIMESTAMP = null
            AS $$
            BEGIN
                UPDATE ""{Table.TableName}""
                SET 
                    ""FriendlyName"" = COALESCE(p_FriendlyName, ""FriendlyName""), 
                    ""OwnerId"" = COALESCE(p_OwnerId, ""OwnerId""), 
                    ""GameId"" = COALESCE(p_GameId, ""GameId""),
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