using Domain.Enums.Lifecycle;

namespace Domain.DatabaseEntities.Lifecycle;

public class AuditTrailDb
{
    public Guid Id { get; set; }
    public string TableName { get; set; } = null!;
    public Guid RecordId { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public AuditAction Action { get; set; }
    public string? Before { get; set; }
    public string After { get; set; } = null!;
}