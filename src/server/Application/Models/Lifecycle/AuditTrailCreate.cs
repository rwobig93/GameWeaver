using Domain.Enums.Lifecycle;

namespace Application.Models.Lifecycle;

public class AuditTrailCreate
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string TableName { get; set; } = null!;
    public Guid RecordId { get; set; }
    public Guid ChangedBy { get; set; } = Guid.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public AuditAction Action { get; set; }
    public string Before { get; set; } = "{}";
    public string After { get; set; } = "{}";
}