using Domain.Enums;

namespace Domain.Models.GameServer;

public class SoftwareUpdateStatus
{
    public string Name { get; set; } = "";
    public int SteamId { get; set; }
    public string UpdateUrl { get; set; } = "";
    public string CurrentVersion { get; set; } = "";
    public string NewVersion { get; set; } = "";
    public bool UpdateAvailable => CurrentVersion != NewVersion;
    public SoftwareType Type { get; set; }
    public GameSource Source { get; set; }
}