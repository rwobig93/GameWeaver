namespace Application.Models.GameServer.GameUpdate;

public class GameUpdateCreate
{
    public Guid GameId { get; set; }
    public bool SupportsWindows { get; set; }
    public bool SupportsLinux { get; set; }
    public bool SupportsMac { get; set; }
    public string BuildVersion { get; set; } = null!;
    public DateTime BuildVersionReleased { get; set; }
}