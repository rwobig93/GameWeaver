using Application.Database;
using Application.Database.Providers;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.Identity;

public class AppUserPreferencesTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "AppUserPreferences";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(AppUserPreferencesTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 3,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [OwnerId] UNIQUEIDENTIFIER NULL,
                    [ThemePreference] INT NULL,
                    [DrawerDefaultOpen] BIT NULL,
                    [CustomThemeOne] NVARCHAR(1024) NULL,
                    [CustomThemeTwo] NVARCHAR(1024) NULL,
                    [CustomThemeThree] NVARCHAR(1024) NULL
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
                @OwnerId UNIQUEIDENTIFIER
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
                WHERE OwnerId = @OwnerId;
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
    
    public static readonly SqlStoredProcedure GetByOwnerId = new()
    {
        Table = Table,
        Action = "GetByOwnerId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByOwnerId]
                @OwnerId UNIQUEIDENTIFIER
            AS
            begin
                SELECT *
                FROM dbo.[{Table.TableName}]
                WHERE OwnerId = @OwnerId;
            end"
    };

    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @OwnerId UNIQUEIDENTIFIER,
                @ThemePreference INT,
                @DrawerDefaultOpen BIT,
                @CustomThemeOne NVARCHAR(1024),
                @CustomThemeTwo NVARCHAR(1024),
                @CustomThemeThree NVARCHAR(1024)
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (OwnerId, ThemePreference, DrawerDefaultOpen, CustomThemeOne, CustomThemeTwo, CustomThemeThree)
                OUTPUT INSERTED.Id
                VALUES (@OwnerId, @ThemePreference, @DrawerDefaultOpen, @CustomThemeOne, @CustomThemeTwo, @CustomThemeThree);
            end"
    };

    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Update]
                @Id UNIQUEIDENTIFIER,
                @OwnerId UNIQUEIDENTIFIER = null,
                @ThemePreference INT = null,
                @DrawerDefaultOpen BIT = null,
                @CustomThemeOne NVARCHAR(1024) = null,
                @CustomThemeTwo NVARCHAR(1024) = null,
                @CustomThemeThree NVARCHAR(1024) = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET OwnerId = COALESCE(@OwnerId, OwnerId), ThemePreference = COALESCE(@ThemePreference, ThemePreference),
                    DrawerDefaultOpen = COALESCE(@DrawerDefaultOpen, DrawerDefaultOpen), CustomThemeOne = COALESCE(@CustomThemeOne, CustomThemeOne),
                    CustomThemeTwo = COALESCE(@CustomThemeTwo, CustomThemeTwo), CustomThemeThree = COALESCE(@CustomThemeThree, CustomThemeThree)
                WHERE Id = COALESCE(@Id, Id);
            end"
    };
}