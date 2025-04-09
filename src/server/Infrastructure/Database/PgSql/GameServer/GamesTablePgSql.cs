using Application.Database;
using Application.Database.PgSql;
using Application.Database.Postgres;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.PgSql.GameServer;

public class GamesTablePgSql: IPgSqlEnforcedEntity
{
    private const string TableName = "Games";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(GamesTablePgSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 5,
        TableName = TableName,
        SqlStatement = @$"
            CREATE TABLE IF NOT EXISTS ""{TableName}"" (
                ""Id"" UUID PRIMARY KEY,
                ""FriendlyName"" VARCHAR(128) NOT NULL,
                ""SteamName"" VARCHAR(128) NOT NULL,
                ""SteamGameId"" INTEGER NULL,
                ""SteamToolId"" INTEGER NULL,
                ""DefaultGameProfileId"" UUID NOT NULL,
                ""LatestBuildVersion"" VARCHAR(128) NOT NULL,
                ""UrlBackground"" VARCHAR(256) NOT NULL,
                ""UrlLogo"" VARCHAR(256) NOT NULL,
                ""UrlLogoSmall"" VARCHAR(256) NOT NULL,
                ""UrlWebsite"" VARCHAR(256) NOT NULL,
                ""ControllerSupport"" VARCHAR(128) NOT NULL,
                ""DescriptionShort"" VARCHAR(256) NOT NULL,
                ""DescriptionLong"" VARCHAR(4000) NOT NULL,
                ""DescriptionAbout"" VARCHAR(2048) NOT NULL,
                ""PriceInitial"" VARCHAR(128) NOT NULL,
                ""PriceCurrent"" VARCHAR(128) NOT NULL,
                ""PriceDiscount"" INTEGER NOT NULL,
                ""MetaCriticScore"" INTEGER NOT NULL,
                ""UrlMetaCriticPage"" VARCHAR(128) NOT NULL,
                ""RequirementsPcMinimum"" VARCHAR(128) NOT NULL,
                ""RequirementsPcRecommended"" VARCHAR(128) NOT NULL,
                ""RequirementsMacRecommended"" VARCHAR(128) NOT NULL,
                ""RequirementsLinuxMinimum"" VARCHAR(128) NOT NULL,
                ""RequirementsMacMinimum"" VARCHAR(128) NOT NULL,
                ""RequirementsLinuxRecommended"" VARCHAR(128) NOT NULL,
                ""CreatedBy"" UUID NOT NULL,
                ""CreatedOn"" TIMESTAMP NOT NULL,
                ""LastModifiedBy"" VARCHAR(128) NULL,
                ""LastModifiedOn"" TIMESTAMP NULL,
                ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                ""DeletedOn"" TIMESTAMP  NULL,
                ""SupportsWindows"" INTEGER NOT NULL,
                ""SupportsLinux"" INTEGER NOT NULL,
                ""SupportsMac"" INTEGER NOT NULL,
                ""SourceType"" INTEGER NOT NULL,
                ""ManualFileRecordId"" UUID NULL,
                ""ManualVersionUrlCheck"" VARCHAR(1024) NULL,
                ""ManualVersionUrlDownload"" VARCHAR(1024) NULL
            );"
    };
    
    public static readonly SqlStoredProcedure Delete = new()
    {
        Table = Table,
        Action = "Delete",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp_{Table.TableName}_Delete""(
                    p_Id UUID,
                    p_DeletedBy UUID,
                    p_DeletedOn TIMESTAMP
                )
            LANGUAGE plpgsql
            AS $$
                BEGIN
                    UPDATE ""{Table.TableName}""
                    SET ""IsDeleted"" = TRUE, ""DeletedOn"" = p_DeletedOn, ""LastModifiedBy"" = p_DeletedBy
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
                WHERE ""IsDeleted"" = 0
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
                p_Offset INT,
                p_PageSize INT,
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                Open p_ FOR
                SELECT COUNT(*) OVER() AS ""TotalCount"", *
                FROM ""{Table.TableName}""
                WHERE ""IsDeleted"" = 0
                ORDER BY ""FriendlyName"" ASC OFFSET ""Offset"" LIMIT p_PageSize;
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
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""Id"" = p_Id
                ORDER BY ""CreatedOn"" DESC
                LIMIT 1;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetBySteamName = new()
    {
        Table = Table,
        Action = "GetBySteamName",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetBySteamName"" (
                p_SteamName VARCHAR(128),
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""SteamName"" = p_SteamName AND ""IsDeleted"" = 0
                ORDER BY ""CreatedOn"" DESC;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetByFriendlyName = new()
    {
        Table = Table,
        Action = "GetByFriendlyName",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetByFriendlyName"" (
                p_FriendlyName VARCHAR(128),
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""FriendlyName"" = p_FriendlyName AND ""IsDeleted"" = 0
                ORDER BY ""CreatedOn"" DESC;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetBySteamGameId = new()
    {
        Table = Table,
        Action = "GetBySteamGameId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetBySteamGameId"" (
                p_SteamGameId INT,
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""SteamGameId"" = p_SteamGameId AND ""IsDeleted"" = 0
                ORDER BY ""CreatedOn"" DESC;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetBySteamToolId = new()
    {
        Table = Table,
        Action = "GetBySteamToolId",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetBySteamToolId"" (
                p_SteamToolId INT,
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""SteamToolId"" = p_SteamToolId AND ""IsDeleted"" = 0
                ORDER BY ""CreatedOn"" DESC;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure GetBySourceType = new()
    {
        Table = Table,
        Action = "GetBySourceType",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_GetBySourceType"" (
                p_SourceType INT,
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT *
                FROM ""{Table.TableName}""
                WHERE ""SourceType"" = p_SourceType AND ""IsDeleted"" = 0
                ORDER BY ""FriendlyName"" ASC;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Insert"" (
                p_Id UUID,
                p_FriendlyName VARCHAR(128),
                p_SteamName VARCHAR(128),
                p_SteamGameId INT,
                p_SteamToolId INT,
                p_DefaultGameProfileId UUID,
                p_LatestBuildVersion VARCHAR(128),
                p_UrlBackground VARCHAR(258),
                p_UrlLogo VARCHAR(256),
                p_UrlLogoSmall VARCHAR(256),
                p_UrlWebsite VARCHAR(256),
                p_ControllerSupport VARCHAR(128),
                p_DescriptionShort VARCHAR(256),
                p_DescriptionLong VARCHAR(4000),
                p_DescriptionAbout VARCHAR(2048),
                p_PriceInitial VARCHAR(128),
                p_PriceCurrent VARCHAR(128),
                p_PriceDiscount INT,
                p_MetaCriticScore INT,
                p_UrlMetaCriticPage VARCHAR(128),
                p_RequirementsPcMinimum VARCHAR(128),
                p_RequirementsPcRecommended VARCHAR(128),
                p_RequirementsMacMinimum VARCHAR(128),
                p_RequirementsMacRecommended VARCHAR(128),
                p_RequirementsLinuxMinimum VARCHAR(128),
                p_RequirementsLinuxRecommended VARCHAR(128),
                p_CreatedBy UUID,
                p_CreatedOn TIMESTAMP,
                p_LastModifiedBy UUID,
                p_LastModifiedOn TIMESTAMP,
                p_IsDeleted BOOLEAN,
                p_SupportsWindows BOOLEAN,
                p_SupportsLinux BOOLEAN,
                p_SupportsMac BOOLEAN,
                p_SourceType INT,
                p_ManualFileRecordId UUID,
                p_ManualVersionUrlCheck VARCHAR(1024),
                p_ManualVersionUrlDownload VARCHAR(1024),
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                INSERT INTO ""{Table.TableName}""  (
                    ""Id"", ""FriendlyName"", ""SteamName"", ""SteamGameId"", ""SteamToolId"", ""DefaultGameProfileId"", ""LatestBuildVersion"", ""UrlBackground"", ""UrlLogo"",
                    ""UrlLogoSmall"", ""UrlWebsite"", ""ControllerSupport"", ""DescriptionShort"", ""DescriptionLong"", ""DescriptionAbout"", ""PriceInitial"", ""PriceCurrent"",
                    ""PriceDiscount"", ""MetaCriticScore"", ""UrlMetaCriticPage"", ""RequirementsPcMinimum"", ""RequirementsPcRecommended"", ""RequirementsMacMinimum"",
                    ""RequirementsMacRecommended"", ""RequirementsLinuxMinimum"", ""RequirementsLinuxRecommended"", ""CreatedBy"", ""CreatedOn"", ""LastModifiedBy"",
                    ""LastModifiedOn"", ""IsDeleted"", ""SupportsWindows"", ""SupportsLinux"", ""SupportsMac"", ""SourceType"", ""ManualFileRecordId"", ""ManualVersionUrlCheck"",
                    ""ManualVersionUrlDownload""
                )
                VALUES (
                    p_Id, p_FriendlyName, p_SteamName, p_SteamGameId, p_SteamToolId, p_DefaultGameProfileId, p_LatestBuildVersion, p_UrlBackground, p_UrlLogo,
                    p_UrlLogoSmall, p_UrlWebsite, p_ControllerSupport, p_DescriptionShort, p_DescriptionLong, p_DescriptionAbout, p_PriceInitial, p_PriceCurrent,
                    p_PriceDiscount, p_MetaCriticScore, p_UrlMetaCriticPage, p_RequirementsPcMinimum, p_RequirementsPcRecommended, p_RequirementsMacMinimum,
                    p_RequirementsMacRecommended, p_RequirementsLinuxMinimum, p_RequirementsLinuxRecommended, p_CreatedBy, p_CreatedOn, p_LastModifiedBy,
                    p_LastModifiedOn, p_IsDeleted, p_SupportsWindows, p_SupportsLinux, p_SupportsMac, p_SourceType, p_ManualFileRecordId, p_ManualVersionUrlCheck,
                    p_ManualVersionUrlDownload
                )
                RETURNING ""Id"" INTO p_;
            END;
            $$"
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
                WHERE ""IsDeleted"" = 0
                    AND (
                        ""Id""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""FriendlyName"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamName"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamGameId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamToolId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""LatestBuildVersion"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""DescriptionShort"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""ManualFileRecordId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    )
                ORDER BY ""FriendlyName"" ASC;
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
                INOUT p_ REFCURSOR DEFAULT NULL
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_ FOR
                SELECT COUNT(*) OVER() AS TotalCount, *
                FROM ""{Table.TableName}"" 
                WHERE ""IsDeleted"" = 0
                    AND (
                        ""Id""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""FriendlyName"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamName"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamGameId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""SteamToolId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                        OR ""LatestBuildVersion"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""DescriptionShort"" ILIKE '%' || p_SearchTerm || '%'
                        OR ""ManualFileRecordId""::TEXT ILIKE '%' || p_SearchTerm || '%'
                    )
                ORDER BY ""FriendlyName"" ASC OFFSET p_Offset LIMIT p_PageSize;
            END;
            $$;"
    };
    
    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR REPLACE PROCEDURE ""sp{Table.TableName}_Update"" (
                p_Id UUID,
                p_FriendlyName VARCHAR(128) DEFAULT NULL,
                p_SteamName VARCHAR(128) DEFAULT NULL,
                p_SteamGameId INT DEFAULT NULL,
                p_SteamToolId INT DEFAULT NULL,
                p_DefaultGameProfileId UUID DEFAULT NULL,
                p_LatestBuildVersion VARCHAR(128) DEFAULT NULL,
                p_UrlBackground VARCHAR(258) DEFAULT NULL,
                p_UrlLogo VARCHAR(256) DEFAULT NULL,
                p_UrlLogoSmall VARCHAR(256) DEFAULT NULL,
                p_UrlWebsite VARCHAR(256) DEFAULT NULL,
                p_ControllerSupport VARCHAR(128) DEFAULT NULL,
                p_DescriptionShort VARCHAR(256) DEFAULT NULL,
                p_DescriptionLong VARCHAR(4000) DEFAULT NULL,
                p_DescriptionAbout VARCHAR(2048) DEFAULT NULL,
                p_PriceInitial VARCHAR(128) DEFAULT NULL,
                p_PriceCurrent VARCHAR(128) DEFAULT NULL,
                p_PriceDiscount INT DEFAULT NULL,
                p_MetaCriticScore INT DEFAULT NULL,
                p_UrlMetaCriticPage VARCHAR(128) DEFAULT NULL,
                p_RequirementsPcMinimum VARCHAR(128) DEFAULT NULL,
                p_RequirementsPcRecommended VARCHAR(128) DEFAULT NULL,
                p_RequirementsMacMinimum VARCHAR(128) DEFAULT NULL,
                p_RequirementsMacRecommended VARCHAR(128) DEFAULT NULL,
                p_RequirementsLinuxMinimum VARCHAR(128) DEFAULT NULL,
                p_RequirementsLinuxRecommended VARCHAR(128) DEFAULT NULL,
                p_CreatedBy UUID DEFAULT NULL,
                p_CreatedOn TIMESTAMP DEFAULT NULL,
                p_LastModifiedBy UUID DEFAULT NULL,
                p_LastModifiedOn TIMESTAMP DEFAULT NULL,
                p_IsDeleted BOOLEAN DEFAULT NULL,
                p_DeletedOn TIMESTAMP DEFAULT NULL,
                p_SupportsWindows BOOLEAN DEFAULT NULL,
                p_SupportsLinux BOOLEAN DEFAULT NULL,
                p_SupportsMac BOOLEAN DEFAULT NULL,
                p_SourceType INT DEFAULT NULL,
                p_ManualFileRecordId UUID DEFAULT NULL,
                p_ManualVersionUrlCheck VARCHAR(1024) DEFAULT NULL,
                p_ManualVersionUrlDownload VARCHAR(1024) DEFAULT NULL            
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE ""{Table.TableName}""
                SET ""FriendlyName"" = COALESCE(p_FriendlyName, ""FriendlyName""), 
                    ""SteamName"" = COALESCE(p_SteamName, ""SteamName""),
                    ""SteamGameId"" = COALESCE(p_SteamGameId, ""SteamGameId""), 
                    ""SteamToolId"" = COALESCE(p_SteamToolId, ""SteamToolId""),
                    ""DefaultGameProfileId"" = COALESCE(p_DefaultGameProfileId, ""DefaultGameProfileId""), 
                    ""UrlBackground"" = COALESCE(p_UrlBackground, ""UrlBackground""),
                    ""LatestBuildVersion"" = COALESCE(p_LatestBuildVersion, ""LatestBuildVersion""), 
                    ""UrlLogo"" = COALESCE(p_UrlLogo, ""UrlLogo""),
                    ""UrlLogoSmall"" = COALESCE(p_UrlLogoSmall, ""UrlLogoSmall""), 
                    ""UrlWebsite"" = COALESCE(p_UrlWebsite, ""UrlWebsite""),
                    ""ControllerSupport"" = COALESCE(p_ControllerSupport, ""ControllerSupport""), 
                    ""DescriptionShort"" = COALESCE(p_DescriptionShort, ""DescriptionShort""),
                    ""DescriptionLong"" = COALESCE(p_DescriptionLong, ""DescriptionLong""), 
                    ""DescriptionAbout"" = COALESCE(p_DescriptionAbout, ""DescriptionAbout""),
                    ""PriceInitial"" = COALESCE(p_PriceInitial, ""PriceInitial""), 
                    ""PriceCurrent"" = COALESCE(p_PriceCurrent, ""PriceCurrent""),
                    ""PriceDiscount"" = COALESCE(p_PriceDiscount, ""PriceDiscount""), 
                    ""MetaCriticScore"" = COALESCE(p_MetaCriticScore, ""MetaCriticScore""),
                    ""UrlMetaCriticPage"" = COALESCE(p_UrlMetaCriticPage, ""UrlMetaCriticPage""), 
                    ""RequirementsPcMinimum"" = COALESCE(p_RequirementsPcMinimum, ""RequirementsPcMinimum""),
                    ""RequirementsPcRecommended"" = COALESCE(p_RequirementsPcRecommended, ""RequirementsPcRecommended""),
                    ""RequirementsMacMinimum"" = COALESCE(p_RequirementsMacMinimum, ""RequirementsMacMinimum""),
                    ""RequirementsMacRecommended"" = COALESCE(p_RequirementsMacRecommended, ""RequirementsMacRecommended""),
                    ""RequirementsLinuxMinimum"" = COALESCE(p_RequirementsLinuxMinimum, ""RequirementsLinuxMinimum""),
                    ""RequirementsLinuxRecommended"" = COALESCE(p_RequirementsLinuxRecommended, ""RequirementsLinuxRecommended""), 
                    ""CreatedBy"" = COALESCE(p_CreatedBy, ""CreatedBy""),
                    ""CreatedOn"" = COALESCE(p_CreatedOn, ""CreatedOn""), 
                    ""LastModifiedBy"" = COALESCE(p_LastModifiedBy, ""LastModifiedBy""),
                    ""LastModifiedOn"" = COALESCE(p_LastModifiedOn, ""LastModifiedOn""), 
                    ""IsDeleted"" = COALESCE(p_IsDeleted, ""IsDeleted""), 
                    ""DeletedOn"" = COALESCE(p_DeletedOn, ""DeletedOn""),
                    ""SupportsWindows"" = COALESCE(p_SupportsWindows, ""SupportsWindows""), 
                    ""SupportsLinux"" = COALESCE(p_SupportsLinux, ""SupportsLinux""),
                    ""SupportsMac"" = COALESCE(p_SupportsMac, ""SupportsMac""), 
                    ""SourceType"" = COALESCE(p_SourceType, ""SourceType""),
                    ""ManualFileRecordId"" = COALESCE(p_ManualFileRecordId, ""ManualFileRecordId""), 
                    ""ManualVersionUrlCheck"" = COALESCE(p_ManualVersionUrlCheck, ""ManualVersionUrlCheck""),
                    ""ManualVersionUrlDownload"" = COALESCE(p_ManualVersionUrlDownload, ""ManualVersionUrlDownload"")
                WHERE ""Id"" = p_Id;
                
            END;
            $$;"
    };
}