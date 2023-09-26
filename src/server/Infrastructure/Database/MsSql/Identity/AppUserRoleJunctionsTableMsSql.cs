using Application.Database;
using Application.Database.Providers;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.Identity;

public class AppUserRoleJunctionsTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "AppUserRoleJunctions";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(AppUserRoleJunctionsTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 3,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [UserId] UNIQUEIDENTIFIER NOT NULL,
                    [RoleId] UNIQUEIDENTIFIER NOT NULL,
                    CONSTRAINT User_Role_PK PRIMARY KEY (UserId, RoleId),
                    CONSTRAINT FK_User
                        FOREIGN KEY (UserId) REFERENCES dbo.[AppUsers] (Id) ON UPDATE CASCADE,
                    CONSTRAINT FK_Role
                        FOREIGN KEY (RoleId) REFERENCES dbo.[AppRoles] (Id) ON UPDATE CASCADE
                )
            end"
    };
    
    public static readonly SqlStoredProcedure Delete = new()
    {
        Table = Table,
        Action = "Delete",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Delete]
                @UserId UNIQUEIDENTIFIER,
                @RoleId UNIQUEIDENTIFIER
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
                WHERE UserId = @UserId AND
                      RoleId = @RoleId;
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
                SELECT *
                FROM dbo.[{Table.TableName}];
            end"
    };
    
    public static readonly SqlStoredProcedure GetByUserRoleId = new()
    {
        Table = Table,
        Action = "GetByUserRoleId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByUserRoleId]
                @UserId UNIQUEIDENTIFIER,
                @RoleId UNIQUEIDENTIFIER
            AS
            begin
                SELECT TOP 1 *
                FROM dbo.[{Table.TableName}]
                WHERE UserId = @UserId AND
                      RoleId = @RoleId
                ORDER BY UserId;
            end"
    };
    
    public static readonly SqlStoredProcedure GetRolesOfUser = new()
    {
        Table = Table,
        Action = "GetRolesOfUser",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetRolesOfUser]
                @UserId UNIQUEIDENTIFIER
            AS
            begin
                SELECT r.*
                FROM dbo.[{Table.TableName}] ur
                JOIN dbo.[{AppRolesTableMsSql.Table.TableName}] r ON r.Id = ur.RoleId
                WHERE ur.UserId = @UserId;
            end"
    };
    
    public static readonly SqlStoredProcedure GetUsersOfRole = new()
    {
        Table = Table,
        Action = "GetUsersOfRole",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetUsersOfRole]
                @RoleId UNIQUEIDENTIFIER
            AS
            begin
                SELECT u.*
                FROM dbo.[{Table.TableName}] ur
                JOIN dbo.[{AppUsersTableMsSql.Table.TableName}] u ON u.Id = ur.UserId
                WHERE ur.RoleId = @RoleId;
            end"
    };
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @UserId UNIQUEIDENTIFIER,
                @RoleId UNIQUEIDENTIFIER
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (UserId, RoleId)
                VALUES (@UserId, @RoleId);
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
                set nocount on;
                
                SELECT *
                FROM dbo.[{Table.TableName}]
                WHERE UserId LIKE '%' + @SearchTerm + '%'
                    OR RoleId LIKE '%' + @SearchTerm + '%';
            end"
    };
}