using Application.Constants.Identity;
using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.Identity;

public class AppPermissionsTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "AppPermissions";
    
    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(AppPermissionsTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 3,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [RoleId] UNIQUEIDENTIFIER NULL,
                    [UserId] UNIQUEIDENTIFIER NULL,
                    [ClaimType] NVARCHAR(256) NULL,
                    [ClaimValue] NVARCHAR(1024) NULL,
                    [Name] NVARCHAR(256) NOT NULL,
                    [Group] NVARCHAR(256) NOT NULL,
                    [Access] NVARCHAR(256) NOT NULL,
                    [Description] NVARCHAR(4000) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [CreatedOn] datetime2 NOT NULL,
                    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
                    [LastModifiedOn] datetime2 NULL
                )
            end"
    };
    
    public static readonly SqlStoredProcedure Delete = new()
    {
        Table = Table,
        Action = "Delete",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Delete]
                @Id UNIQUEIDENTIFIER
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
                WHERE Id = @Id;
            end"
    };
    
    public static readonly SqlStoredProcedure DeleteForUser = new()
    {
        Table = Table,
        Action = "DeleteForUser",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_DeleteForUser]
                @UserId UNIQUEIDENTIFIER
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
                WHERE UserId = @UserId;
            end"
    };
    
    public static readonly SqlStoredProcedure DeleteForRole = new()
    {
        Table = Table,
        Action = "DeleteForRole",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_DeleteForRole]
                @RoleId UNIQUEIDENTIFIER
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
                WHERE RoleId = @RoleId;
            end"
    };
    
    public static readonly SqlStoredProcedure GetAll = new()
    {
        Table = Table,
        Action = "GetAll",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAll]
            AS
            begin
                SELECT p.*
                FROM dbo.[{Table.TableName}] p;
            end"
    };

    public static readonly SqlStoredProcedure GetAllPaginated = new()
    {
        Table = Table,
        Action = "GetAllPaginated",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllPaginated]
                @Offset INT,
                @PageSize INT
            AS
            begin
                SELECT COUNT(*) OVER() AS TotalCount, p.*
                FROM dbo.[{Table.TableName}] p
                ORDER BY p.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };

    public static readonly SqlStoredProcedure GetAllUsersByClaimValue = new()
    {
        Table = Table,
        Action = "GetAllUsersByClaimValue",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllUsersByClaimValue]
            AS
            begin
                SELECT u.*, s.PasswordHash, s.PasswordSalt, s.TwoFactorEnabled, s.TwoFactorKey, s.AuthState, s.AuthStateTimestamp,
                        s.BadPasswordAttempts, s.LastBadPassword, s.LastFullLogin
                FROM dbo.[{Table.TableName}] p
                JOIN dbo.[{AppUsersTableMsSql.Table.TableName}] u ON u.Id = p.UserId
                JOIN dbo.[{AppUserSecurityAttributesTableMsSql.Table.TableName}] s ON u.Id = s.OwnerId
                WHERE u.IsDeleted = 0;
            end"
    };

    public static readonly SqlStoredProcedure GetAllRolesByClaimValue = new()
    {
        Table = Table,
        Action = "GetAllRolesByClaimValue",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllRolesByClaimValue]
            AS
            begin
                SELECT r.*
                FROM dbo.[{Table.TableName}] p
                JOIN dbo.[{AppRolesTableMsSql.Table.TableName}] r ON r.Id = p.RoleId;
            end"
    };
    
    public static readonly SqlStoredProcedure GetById = new()
    {
        Table = Table,
        Action = "GetById",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetById]
                @Id UNIQUEIDENTIFIER
            AS
            begin
                SELECT TOP 1 *
                FROM dbo.[{Table.TableName}]
                WHERE Id = @Id
                ORDER BY Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByName = new()
    {
        Table = Table,
        Action = "GetByName",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByName]
                @Name NVARCHAR(256)
            AS
            begin
                SELECT TOP 1 *
                FROM dbo.[{Table.TableName}]
                WHERE Name = @Name
                ORDER BY Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByGroup = new()
    {
        Table = Table,
        Action = "GetByGroup",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByGroup]
                @Group NVARCHAR(256)
            AS
            begin
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.[Group] = @Group;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByAccess = new()
    {
        Table = Table,
        Action = "GetByAccess",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByAccess]
                @Access NVARCHAR(256)
            AS
            begin
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.Access = @Access;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByClaimValue = new()
    {
        Table = Table,
        Action = "GetByClaimValue",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByClaimValue]
                @ClaimValue NVARCHAR(1024)
            AS
            begin
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.ClaimValue = @ClaimValue;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByRoleId = new()
    {
        Table = Table,
        Action = "GetByRoleId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByRoleId]
                @RoleId UNIQUEIDENTIFIER
            AS
            begin
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.RoleId = @RoleId;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByRoleIdAndValue = new()
    {
        Table = Table,
        Action = "GetByRoleIdAndValue",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByRoleIdAndValue]
                @RoleId UNIQUEIDENTIFIER,
                @ClaimValue NVARCHAR(1024)
            AS
            begin
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.RoleId = @RoleId AND p.ClaimValue = @ClaimValue;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByUserId = new()
    {
        Table = Table,
        Action = "GetByUserId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByUserId]
                @UserId UNIQUEIDENTIFIER
            AS
            begin
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.UserId = @UserId;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByUserIdAndValue = new()
    {
        Table = Table,
        Action = "GetByUserIdAndValue",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByUserIdAndValue]
                @UserId UNIQUEIDENTIFIER,
                @ClaimValue NVARCHAR(1024)
            AS
            begin
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.UserId = @UserId AND p.ClaimValue = @ClaimValue;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @RoleId UNIQUEIDENTIFIER,
                @UserId UNIQUEIDENTIFIER,
                @ClaimType NVARCHAR(256),
                @ClaimValue NVARCHAR(1024),
                @Name NVARCHAR(256),
                @Group NVARCHAR(256),
                @Access NVARCHAR(256),
                @Description NVARCHAR(4000),
                @CreatedBy UNIQUEIDENTIFIER,
                @CreatedOn datetime2,
                @LastModifiedBy UNIQUEIDENTIFIER,
                @LastModifiedOn datetime2
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (RoleId, UserId, Name, [Group], Access, ClaimType, ClaimValue, Description, CreatedBy, CreatedOn,
                LastModifiedBy, LastModifiedOn)
                OUTPUT INSERTED.Id
                VALUES (@RoleId, @UserId, @Name, @Group, @Access, @ClaimType, @ClaimValue, @Description, @CreatedBy, @CreatedOn, @LastModifiedBy,
                @LastModifiedOn);
            end"
    };
    
    public static readonly SqlStoredProcedure Search = new()
    {
        Table = Table,
        Action = "Search",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Search]
                @SearchTerm NVARCHAR(256)
            AS
            begin
                SET nocount on;
                
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.Description LIKE '%' + @SearchTerm + '%'
                    OR p.RoleId LIKE '%' + @SearchTerm + '%'
                    OR p.UserId LIKE '%' + @SearchTerm + '%'
                    OR p.ClaimValue LIKE '%' + @SearchTerm + '%';
            end"
    };
    
    public static readonly SqlStoredProcedure SearchPaginated = new()
    {
        Table = Table,
        Action = "SearchPaginated",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_SearchPaginated]
                @SearchTerm NVARCHAR(256),
                @Offset INT,
                @PageSize INT
            AS
            begin
                SELECT COUNT(*) OVER() AS TotalCount, p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.Description LIKE '%' + @SearchTerm + '%'
                    OR p.RoleId LIKE '%' + @SearchTerm + '%'
                    OR p.UserId LIKE '%' + @SearchTerm + '%'
                    OR p.ClaimValue LIKE '%' + @SearchTerm + '%'
                ORDER BY p.Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            end"
    };
    
    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Update]
                @Id UNIQUEIDENTIFIER,
                @RoleId UNIQUEIDENTIFIER = null,
                @UserId UNIQUEIDENTIFIER = null,
                @ClaimType NVARCHAR(256) = null,
                @ClaimValue NVARCHAR(1024) = null,
                @Name NVARCHAR(256) = null,
                @Group NVARCHAR(256) = null,
                @Access NVARCHAR(256) = null,
                @Description NVARCHAR(4000) = null,
                @CreatedBy UNIQUEIDENTIFIER = null,
                @CreatedOn datetime2 = null,
                @LastModifiedBy UNIQUEIDENTIFIER = null,
                @LastModifiedOn datetime2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET RoleId = COALESCE(@RoleId, RoleId), UserID = COALESCE(@UserId, UserId), ClaimType = COALESCE(@ClaimType, ClaimType),
                    ClaimValue = COALESCE(@ClaimValue, ClaimValue), Name = COALESCE(@Name, Name), [Group] = COALESCE(@Group, [Group]),
                    Access = COALESCE(@Access, Access), Description = COALESCE(@Description, Description),
                    CreatedBy = COALESCE(@CreatedBy, CreatedBy), CreatedOn = COALESCE(@CreatedOn, CreatedOn),
                    LastModifiedBy = COALESCE(@LastModifiedBy, LastModifiedBy), LastModifiedOn = COALESCE(@LastModifiedOn, LastModifiedOn)
                WHERE Id = COALESCE(@Id, Id);
            end"
    };
    
    public static readonly SqlStoredProcedure GetDynamicByTypeAndName = new()
    {
        Table = Table,
        Action = "GetDynamicByTypeAndName",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetDynamicByTypeAndName]
                @Group NVARCHAR(256),
                @Name NVARCHAR(256)
            AS
            begin
                SELECT p.*
                FROM dbo.[{Table.TableName}] p
                WHERE p.[ClaimType] = '{ClaimConstants.DynamicPermission}'
                    AND p.[Group] = @Group
                    AND p.[Name] = @Name;
            end"
    };
}