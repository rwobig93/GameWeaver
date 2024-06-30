using Domain.Enums.Lifecycle;

namespace Domain.DatabaseEntities.Lifecycle;

public class TroubleshootingRecordDb
{
    public Guid Id { get; set; }
    public TroubleshootEntityType EntityType { get; set; }
    public Guid RecordId { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Message { get; set; } = null!;
    public string? Detail { get; set; }
}