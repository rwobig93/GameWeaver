using Domain.Enums;

namespace Domain.Models.GameServer;

public class GameServerSoftwareUpdate
{
    public string Name { get; set; } = "";
    public int SteamId { get; set; }
    public string UpdateUrl { get; set; } = "";
    public string CurrentVersion { get; set; } = "";
    public string NewVersion { get; set; } = "";
    public GameSource Source { get; set; }
}