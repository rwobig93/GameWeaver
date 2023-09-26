namespace Application.Models.Lifecycle;

public class ServerStateRecordSlim
{
    public Guid Id { get; set; }
    public string AppVersion { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}