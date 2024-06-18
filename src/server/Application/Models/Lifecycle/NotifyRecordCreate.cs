namespace Application.Models.Lifecycle;

public class NotifyRecordCreate
{
    public Guid RecordId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = null!;
    public string? Detail { get; set; } = null;
}