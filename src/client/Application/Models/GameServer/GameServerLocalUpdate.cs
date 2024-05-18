using Domain.Contracts;
using Domain.Enums;
using Domain.Models.GameServer;

namespace Application.Models.GameServer;

public class GameServerLocalUpdate
{
    public Guid Id { get; set; }
    public string? SteamName { get; set; }
    public int? SteamGameId { get; set; }
    public int? SteamToolId { get; set; }
    public string? ServerName { get; set; }
    public string? Password { get; set; }
    public string? PasswordRcon { get; set; }
    public string? PasswordAdmin { get; set; }
    public string? ServerVersion { get; set; }
    public string? IpAddress { get; set; }
    public string? ExtHostname { get; set; }
    public int? PortGame { get; set; }
    public int? PortQuery { get; set; }
    public int? PortRcon { get; set; }
    public bool? Modded { get; set; }
    public string? ManualRootUrl { get; set; }
    public string? ServerProcessName { get; set; }
    public DateTime? LastStateUpdate { get; set; }
    public ServerState? ServerState { get; set; }
    public GameSource? Source { get; set; }
    public SerializableList<Mod>? ModList { get; set; }
    public SerializableList<LocalResource>? Resources { get; set; }
    public SerializableList<SoftwareUpdateStatus>? UpdatesWaiting { get; set; }
}