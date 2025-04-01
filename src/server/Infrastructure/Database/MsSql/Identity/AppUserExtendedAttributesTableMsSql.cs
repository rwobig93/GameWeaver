using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.Identity;

public class AppUserExtendedAttributesTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "AppUserExtendedAttributes";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(AppUserExtendedAttributesTableMsSql).GetDbScriptsFromClass();
    
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
                    [Name] NVARCHAR(256) NOT NULL,
                    [Value] NVARCHAR(512) NOT NULL,
                    [Description] NVARCHAR(1024) NULL,
                    [Type] int NOT NULL
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
                SELECT TOP 1 e.*
                FROM dbo.[{Table.TableName}] e
                WHERE e.Id = @Id
                ORDER BY e.Id;
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
                SELECT e.*
                FROM dbo.[{Table.TableName}] e
                WHERE e.OwnerId = @OwnerId;
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
                SELECT e.*
                FROM dbo.[{Table.TableName}] e
                WHERE e.Name = @Name;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByTypeAndValue = new()
    {
        Table = Table,
        Action = "GetByTypeAndValue",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByTypeAndValue]
                @Type int,
                @Value NVARCHAR(256)
            AS
            begin
                SELECT e.*
                FROM dbo.[{Table.TableName}] e
                WHERE e.Value = @Value AND e.Type = @Type
                ORDER BY e.Id;
            end"
    };
    
    public static readonly SqlStoredProcedure GetByTypeAndValueForOwner = new()
    {
        Table = Table,
        Action = "GetByTypeAndValueForOwner",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByTypeAndValueForOwner]
                @OwnerId UNIQUEIDENTIFIER,
                @Type int,
                @Value NVARCHAR(256)
            AS
            begin
                SELECT e.*
                FROM dbo.[{Table.TableName}] e
                WHERE e.Value = @Value AND e.Type = @Type AND e.OwnerId = @OwnerId
                ORDER BY e.Id;
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
                SELECT e.*
                FROM dbo.[{Table.TableName}] e;
            end"
    };
    
    public static readonly SqlStoredProcedure GetAllOfType = new()
    {
        Table = Table,
        Action = "GetAllOfType",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllOfType]
                @Type int
            AS
            begin
                SELECT e.*
                FROM dbo.[{Table.TableName}] e
                WHERE e.Type = @Type;
            end"
    };
    
    public static readonly SqlStoredProcedure GetAllOfTypeForOwner = new()
    {
        Table = Table,
        Action = "GetAllOfTypeForOwner",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllOfTypeForOwner]
                @OwnerId UNIQUEIDENTIFIER,
                @Type int
            AS
            begin
                SELECT e.*
                FROM dbo.[{Table.TableName}] e
                WHERE e.OwnerId = @OwnerId AND e.Type = @Type;
            end"
    };
    
    public static readonly SqlStoredProcedure GetAllOfNameForOwner = new()
    {
        Table = Table,
        Action = "GetAllOfNameForOwner",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllOfNameForOwner]
                @OwnerId UNIQUEIDENTIFIER,
                @Name NVARCHAR(256)
            AS
            begin
                SELECT e.*
                FROM dbo.[{Table.TableName}] e
                WHERE e.OwnerId = @OwnerId AND e.Name = @Name;
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
                @Name NVARCHAR(256),
                @Value NVARCHAR(512),
                @Description NVARCHAR(1024),
                @Type int
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (Id, OwnerId, Name, Value, Description, Type)
                OUTPUT INSERTED.Id
                values (@Id, @OwnerId, @Name, @Value, @Description, @Type);
            end"
    };
    
    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Update]
                @Id UNIQUEIDENTIFIER,
                @Value NVARCHAR(512) = null,
                @Description NVARCHAR(1024) = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET Value = COALESCE(@Value, Value), Description = COALESCE(@Description, Description)
                WHERE Id = @Id;
            end"
    };
}