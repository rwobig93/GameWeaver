using Application.Database;
using Application.Database.Providers;
using Application.Helpers.Runtime;

namespace Infrastructure.Database.MsSql.Shared;

public class GeneralTableMsSql : IMsSqlEnforcedEntity
{
    private const string TableName = "General";

    public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(GeneralTableMsSql).GetDbScriptsFromClass();
    
    public static readonly SqlStoredProcedure GetRowCount = new()
    {
        Table = new SqlTable() { TableName = TableName },
        Action = "GetRowCount",
        SqlStatement = $@"
            CREATE OR ALTER PROCEDURE [dbo].[sp{TableName}_GetRowCount]
                @TableName NVARCHAR(50)
            AS
            begin
                SELECT SUM(spart.rows)
                FROM sys.partitions spart
                WHERE spart.object_id = object_id(@TableName)
                AND spart.index_id < 2;
            end"
    };
    
    public static readonly SqlStoredProcedure VerifyConnectivity = new()
    {
        Table = new SqlTable() { TableName = TableName },
        Action = "VerifyConnectivity",
        SqlStatement = $@"
            CREATE OR ALTER PROCEDURE [dbo].[sp{TableName}_VerifyConnectivity]
            AS
            begin
                SELECT 1;
            end"
    };
}