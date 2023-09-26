namespace Domain.DatabaseEntities.Lifecycle;

public class ServerStateRecordDb
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string AppVersion { get; set; } = null!;
    public string DatabaseVersion { get; set; } = null!;
}