using Domain.Enums.Lifecycle;

namespace Application.Models.Lifecycle;

public class TroubleshootingRecordCreate
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public TroubleshootEntityType EntityType { get; set; }
    public Guid RecordId { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = null!;
    public string? Detail { get; set; }
}