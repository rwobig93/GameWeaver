namespace Application.Models.Lifecycle;

public class ServerStateRecordCreate
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime Timestamp { get; set; }
    public string AppVersion { get; set; } = null!;
    public string DatabaseVersion { get; set; } = null!;
}