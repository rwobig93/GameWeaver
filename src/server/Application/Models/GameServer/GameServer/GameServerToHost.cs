using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
using Domain.Enums.GameServer;

namespace Application.Models.GameServer.GameServer;

public class GameServerToHost
{
    public Guid Id { get; set; }
    public string SteamName { get; set; } = "";
    public int SteamGameId { get; set; }
    public int SteamToolId { get; set; }
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
    public ConnectivityState ServerState { get; set; }
    public GameSource Source { get; set; }
    public List<ModSlim> ModList { get; set; } = new();
    public List<LocalResourceSlim> Resources { get; set; } = new();
}