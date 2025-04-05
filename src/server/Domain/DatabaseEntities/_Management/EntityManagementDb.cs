using Domain.Enums.Database;

namespace Domain.DatabaseEntities._Management;

public class EntityManagementDb
{
    public string Path { get; set; } = string.Empty;
    public DbResourceType Type { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}