using Application.Database;
using Application.Database.MsSql;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql._Management;

public class EntityManagementMsSql : IMsSqlManagementEntity
{
    private const string TableName = "EntityManagement";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(EntityManagementMsSql).GetDbScriptsFromClass();

    public static readonly SqlTable Table = new()
    {
        EnforcementOrder = 1,
        TableName = TableName,
        SqlStatement = $@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND OBJECT_ID = OBJECT_ID('[dbo].[{TableName}]'))
            begin
                CREATE TABLE [dbo].[{TableName}](
                    [Path] NVARCHAR(128) PRIMARY KEY,
                    [Type] INT NOT NULL,
                    [Hash] NVARCHAR(128) NOT NULL,
                    [AppVersion] NVARCHAR(128) NOT NULL,
                    [LastUpdated] DATETIME2 NOT NULL
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
                SELECT e.*
                FROM dbo.[{Table.TableName}] e;
            end"
    };

    public static readonly SqlStoredProcedure Insert = new()
    {
        Table = Table,
        Action = "Insert",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Insert]
                @Path NVARCHAR(128),
                @Type INT,
                @Hash NVARCHAR(128),
                @AppVersion NVARCHAR(128),
                @LastUpdated DATETIME2
            AS
            begin
                INSERT into dbo.[{Table.TableName}] (Path, Type, Hash, AppVersion, LastUpdated)
                OUTPUT INSERTED.Path
                VALUES (@Path, @Type, @Hash, @AppVersion, @LastUpdated);
            end"
    };

    public static readonly SqlStoredProcedure Update = new()
    {
        Table = Table,
        Action = "Update",
        SqlStatement = @$"
            CREATE OR ALTER PROCEDURE [dbo].[sp{Table.TableName}_Update]
                @Path NVARCHAR(128),
                @Type INT = null,
                @Hash NVARCHAR(128) = null,
                @AppVersion NVARCHAR(128) = null,
                @LastUpdated DATETIME2 = null
            AS
            begin
                UPDATE dbo.[{Table.TableName}]
                SET Type = COALESCE(@Type, Type), Hash = COALESCE(@Hash, Hash), AppVersion = COALESCE(@AppVersion, AppVersion), LastUpdated = COALESCE(@LastUpdated, LastUpdated)
                WHERE Path = @Path;
            end"
    };
}