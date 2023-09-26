using Application.Database;
using Application.Database.Providers;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.Lifecycle;

public class ServerStateRecordsTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "ServerStateRecords";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(ServerStateRecordsTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlTable Table = new()
    {
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Id] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
                    [Timestamp] DATETIME2 NOT NULL,
                    [AppVersion] NVARCHAR(128) NOT NULL,
                    [DatabaseVersion] NVARCHAR(128) NOT NULL
                )
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
                FROM dbo.[{Table.TableName}]
                ORDER BY Timestamp DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetAllBeforeDate = new()
    {
        Table = Table,
        Action = "GetAllBeforeDate",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllBeforeDate]
                @OlderThan DATETIME2
            AS
            begin
                SELECT *
                FROM dbo.[{Table.TableName}]
                WHERE Timestamp < @OlderThan
                ORDER BY Timestamp DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetAllAfterDate = new()
    {
        Table = Table,
        Action = "GetAllAfterDate",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetAllAfterDate]
                @NewerThan DATETIME2
            AS
            begin
                SELECT *
                FROM dbo.[{Table.TableName}]
                WHERE Timestamp > @NewerThan
                ORDER BY Timestamp DESC;
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

    public static readonly SqlStoredProcedure GetLatest = new()
    {
        Table = Table,
        Action = "GetLatest",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetLatest]
            AS
            begin
                SELECT TOP 1 *
                FROM dbo.[{Table.TableName}]
                ORDER BY Timestamp DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetByAppVersion = new()
    {
        Table = Table,
        Action = "GetByAppVersion",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByAppVersion]
                @Version NVARCHAR(128)
            AS
            begin
                SELECT *
                FROM dbo.[{Table.TableName}]
                WHERE AppVersion = @Version
                ORDER BY Timestamp DESC;
            end"
    };

    public static readonly SqlStoredProcedure GetByDatabaseVersion = new()
    {
        Table = Table,
        Action = "GetByDatabaseVersion",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_GetByDatabaseVersion]
                @Version NVARCHAR(128)
            AS
            begin
                SELECT *
                FROM dbo.[{Table.TableName}]
                WHERE DatabaseVersion = @Version
                ORDER BY Timestamp DESC;
            end"
    };

    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @Timestamp DATETIME2,
                @AppVersion NVARCHAR(128),
                @DatabaseVersion NVARCHAR(128)
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (Timestamp, AppVersion, DatabaseVersion)
                OUTPUT INSERTED.Id
                VALUES (@Timestamp, @AppVersion, @DatabaseVersion);
            end"
    };
}