namespace Domain.DatabaseEntities.Lifecycle;

public class NotifyRecordDb
{
    public int Id { get; set; }
    public Guid EntityId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = "";
    public string? Detail { get; set; } = null;
}