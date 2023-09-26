using Domain.Enums.Database;

namespace Domain.DatabaseEntities.Lifecycle;

public class AuditTrailWithUserDb
{
    public Guid Id { get; set; }
    public string TableName { get; set; } = null!;
    public Guid RecordId { get; set; }
    public Guid ChangedBy { get; set; }
    public string ChangedByUsername { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DatabaseActionType Action { get; set; }
    public string? Before { get; set; }
    public string After { get; set; } = null!;
}