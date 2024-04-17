namespace Domain.Models.GameServer;

public class GameInstallState
{
    public DateTime CheckTimestamp { get; set; }
    public string InstallDirectory { get; set; } = null!;
    public Version InstalledVersion { get; set; } = new();
    public Version LatestVersion { get; set; } = new();
    public bool UpdateAvailable { get; set; }
}