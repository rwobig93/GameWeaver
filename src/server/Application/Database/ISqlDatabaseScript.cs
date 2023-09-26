using Application.Models.Database;

namespace Application.Database;

public interface ISqlDatabaseScript
{
    public string FriendlyName { get; }
    public DbResourceType Type { get; }
    public string SqlStatement { get; }
    public string Path { get; }
    
    // Determines the order of database script enforcement by inheriting classes, primarily for foreign keys and database dependencies
    public int EnforcementOrder { get; }
}