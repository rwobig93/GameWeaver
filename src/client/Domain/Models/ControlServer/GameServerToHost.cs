using Domain.Contracts;
using Domain.Enums;
using Domain.Models.GameServer;
using MemoryPack;

namespace Domain.Models.ControlServer;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class GameServerToHost
{
    [MemoryPackOrder(0)]
    public Guid Id { get; set; }
    [MemoryPackOrder(1)]
    public Guid GameId { get; set; }
    [MemoryPackOrder(2)]
    public string SteamName { get; set; } = "";
    [MemoryPackOrder(3)]
    public int SteamGameId { get; set; }
    [MemoryPackOrder(4)]
    public int SteamToolId { get; set; }
    [MemoryPackOrder(5)]
    public string ServerName { get; set; } = "";
    [MemoryPackOrder(6)]
    public string Password { get; set; } = "";
    [MemoryPackOrder(7)]
    public string PasswordRcon { get; set; } = "";
    [MemoryPackOrder(8)]
    public string PasswordAdmin { get; set; } = "";
    [MemoryPackOrder(9)]
    public string ServerVersion { get; set; } = "";
    [MemoryPackOrder(10)]
    public string IpAddress { get; set; } = "";
    [MemoryPackOrder(11)]
    public string ExtHostname { get; set; } = "";
    [MemoryPackOrder(12)]
    public int PortGame { get; set; } = 0;
    [MemoryPackOrder(13)]
    public int PortPeer { get; set; } = 0;
    [MemoryPackOrder(14)]
    public int PortQuery { get; set; } = 0;
    [MemoryPackOrder(15)]
    public int PortRcon { get; set; }
    [MemoryPackOrder(16)]
    public bool Modded { get; set; }
    [MemoryPackOrder(17)]
    public string ManualRootUrl { get; set; } = "";
    [MemoryPackOrder(18)]
    public string ServerProcessName { get; set; } = "";
    [MemoryPackOrder(19)]
    public ServerState ServerState { get; set; }
    [MemoryPackOrder(20)]
    public string RunningConfigHash { get; set; } = "";
    [MemoryPackOrder(21)]
    public string StorageConfigHash { get; set; } = "";
    [MemoryPackOrder(22)]
    public GameSource Source { get; set; }
    [MemoryPackOrder(23)]
    public SerializableList<Mod> ModList { get; set; } = [];
    [MemoryPackOrder(24)]
    public SerializableList<LocalResource> Resources { get; set; } = [];
}