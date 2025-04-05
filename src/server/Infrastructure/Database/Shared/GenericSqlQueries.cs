namespace Infrastructure.Database.Shared;

public static class GenericSqlQueries
{
    public static string TableExists(string tableName) => $"SELECT COUNT (*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
    public static string GetStoredProcedures(string procedurePrefix) => $"SELECT name FROM sys.procedures WHERE name LIKE '%{procedurePrefix}%'";
}