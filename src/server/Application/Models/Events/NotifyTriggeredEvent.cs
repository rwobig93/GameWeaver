namespace Application.Models.Events;

public class NotifyTriggeredEvent
{
    public Guid EntityId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = null!;
    public string? Detail { get; set; } = null;
}