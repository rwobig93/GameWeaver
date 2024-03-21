using Domain.Enums;

namespace Domain.Models.GameServer;

public class GameServerLocal
{
    public Guid Id { get; set; }
    public Guid GameDbId { get; set; }
    public Guid GameProfileDbId { get; set; }
    public string SteamName { get; set; } = "";
    public int SteamGameId { get; set; }
    public int SteamToolId { get; set; }
    public string InstallDirectory { get; set; } = "";
    public List<string> BackupDirectories { get; set; } = new();
    public string ServerName { get; set; } = "";
    public string Password { get; set; } = "";
    public string PasswordRcon { get; set; } = "";
    public string PasswordAdmin { get; set; } = "";
    public string ServerVersion { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string ExtHostname { get; set; } = "";
    public int PortGame { get; set; } = 0;
    public int PortQuery { get; set; } = 0;
    public int PortRcon { get; set; }
    public bool Modded { get; set; }
    public string ManualRootUrl { get; set; } = "";
    public string ServerProcessName { get; set; } = "";
    public DateTime LastStateUpdate { get; set; } = DateTime.Now.ToLocalTime();
    public ServerState ServerState { get; set; } = ServerState.Unknown;
    public GameSource Source { get; set; }
    public List<Mod> ModList { get; set; } = new();
    public List<LocationPointer> Resources { get; set; } = new();
    public List<SoftwareUpdateStatus> UpdatesWaiting { get; set; } = new();
}
