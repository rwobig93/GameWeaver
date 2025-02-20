namespace Application.Models.Events;

public class NotifyTriggeredEvent
{
    public Guid RecordId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = null!;
    public string? Detail { get; set; } = null;
}