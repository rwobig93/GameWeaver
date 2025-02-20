namespace Application.Models.Lifecycle;

public class NotifyRecordSlim
{
    public int Id { get; set; }
    public Guid EntityId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = null!;
    public string? Detail { get; set; } = null;
}