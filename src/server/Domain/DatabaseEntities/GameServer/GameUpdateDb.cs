namespace Domain.DatabaseEntities.GameServer;

public class GameUpdateDb
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public bool SupportsWindows { get; set; }
    public bool SupportsLinux { get; set; }
    public bool SupportsMac { get; set; }
    public string BuildVersion { get; set; } = "";
    public DateTime BuildVersionReleased { get; set; }
}