using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.Identity;

public class AppUserSecurityAttributesTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "AppUserSecurityAttributes";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(AppUserSecurityAttributesTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 3,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                    [OwnerId] UNIQUEIDENTIFIER NOT NULL,
                    [PasswordHash] NVARCHAR(256) NOT NULL,
                    [PasswordSalt] NVARCHAR(256) NOT NULL,
                    [TwoFactorEnabled] BIT NOT NULL,
                    [TwoFactorKey] NVARCHAR(256) NULL,
                    [AuthState] int NOT NULL,
                    [AuthStateTimestamp] datetime2 NULL,
                    [BadPasswordAttempts] int NOT NULL,
                    [LastBadPassword] datetime2 NULL,
                    [LastFullLogin] datetime2 NULL
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
    
    public static readonly SqlStoredProcedure DeleteAllForOwner = new()
    {
        Table = Table,
        Action = "DeleteAllForOwner",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_DeleteAllForOwner]
                @OwnerId UNIQUEIDENTIFIER
            AS
            begin
                DELETE
                FROM dbo.[{Table.TableName}]
                WHERE OwnerId = @OwnerId;
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
                SELECT TOP 1 *
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
    
    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @Id UNIQUEIDENTIFIER,
                @OwnerId UNIQUEIDENTIFIER,
                @PasswordHash NVARCHAR(256),
                @PasswordSalt NVARCHAR(256),
                @TwoFactorEnabled BIT,
                @TwoFactorKey NVARCHAR(256),
                @AuthState int,
                @AuthStateTimestamp datetime2,
                @BadPasswordAttempts int,
                @LastBadPassword datetime2,
                @LastFullLogin datetime2
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (Id, OwnerId, PasswordHash, PasswordSalt, TwoFactorEnabled, TwoFactorKey, AuthState,
                                         AuthStateTimestamp, BadPasswordAttempts, LastBadPassword, LastFullLogin)
                OUTPUT INSERTED.Id
                values (@Id, @OwnerId, @PasswordHash, @PasswordSalt, @TwoFactorEnabled, @TwoFactorKey, @AuthState, @AuthStateTimestamp,
                        @BadPasswordAttempts, @LastBadPassword, @LastFullLogin);
            end"
    };
    
    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Update]
                @OwnerId UNIQUEIDENTIFIER,
                @PasswordHash NVARCHAR(256) = null,
                @PasswordSalt NVARCHAR(256) = null,
                @TwoFactorEnabled BIT = null,
                @TwoFactorKey NVARCHAR(256) = null,
                @AuthState int = null,
                @AuthStateTimestamp datetime2 = null,
                @BadPasswordAttempts int = null,
                @LastBadPassword datetime2 = null,
                @LastFullLogin datetime2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET PasswordHash = COALESCE(@PasswordHash, PasswordHash), PasswordSalt = COALESCE(@PasswordSalt, PasswordSalt),
                    TwoFactorEnabled = COALESCE(@TwoFactorEnabled, TwoFactorEnabled), TwoFactorKey = COALESCE(@TwoFactorKey, TwoFactorKey),
                    AuthState = COALESCE(@AuthState, AuthState), AuthStateTimestamp = COALESCE(@AuthStateTimestamp, AuthStateTimestamp),
                    BadPasswordAttempts = COALESCE(@BadPasswordAttempts, BadPasswordAttempts),
                    LastBadPassword = COALESCE(@LastBadPassword, LastBadPassword), LastFullLogin =  COALESCE(@LastFullLogin, LastFullLogin)
                WHERE OwnerId = @OwnerId;
            end"
    };

    public static readonly SqlStoredProcedure SetOwnerId = new()
    {
        Table = Table,
        Action = "SetOwnerId",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_SetOwnerId]
                @CurrentId UNIQUEIDENTIFIER,
                @NewId UNIQUEIDENTIFIER
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET OwnerId = @NewId
                OUTPUT @NewId
                WHERE OwnerId = @CurrentId;
            end"
    };
}