using Application.Models.Database;

namespace Application.Database;

public class SqlStoredProcedure : ISqlDatabaseScript
{
    public SqlTable Table { get; set; } = null!;
    public string Action { get; set; } = null!;
    public DbResourceType Type => DbResourceType.StoredProcedure;
    public string SqlStatement { get; set; } = null!;
    public int EnforcementOrder { get; init; } = 100;
    public string Path => $"sp{Table.TableName}_{Action}";
    public string FriendlyName => Path;
}