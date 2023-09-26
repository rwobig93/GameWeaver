using Application.Models.Database;

namespace Application.Database;

public class SqlTable : ISqlDatabaseScript
{
    public string TableName { get; set; } = null!;
    public string SqlStatement { get; set; } = null!;
    public string FriendlyName => TableName;
    public DbResourceType Type => DbResourceType.Table;
    public string Path => TableName;
    public int EnforcementOrder { get; init; } = 5;
}