using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
using Domain.Contracts;
using Domain.Enums.GameServer;
using MemoryPack;

namespace Application.Models.GameServer.GameServer;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class GameServerToHost
{
    [MemoryPackOrder(0)]
    public Guid Id { get; set; }
    [MemoryPackOrder(1)]
    public string SteamName { get; set; } = "";
    [MemoryPackOrder(2)]
    public int SteamGameId { get; set; }
    [MemoryPackOrder(3)]
    public int SteamToolId { get; set; }
    [MemoryPackOrder(4)]
    public string ServerName { get; set; } = "";
    [MemoryPackOrder(5)]
    public string Password { get; set; } = "";
    [MemoryPackOrder(6)]
    public string PasswordRcon { get; set; } = "";
    [MemoryPackOrder(7)]
    public string PasswordAdmin { get; set; } = "";
    [MemoryPackOrder(8)]
    public string ServerVersion { get; set; } = "";
    [MemoryPackOrder(9)]
    public string IpAddress { get; set; } = "";
    [MemoryPackOrder(10)]
    public string ExtHostname { get; set; } = "";
    [MemoryPackOrder(11)]
    public int PortGame { get; set; } = 0;
    [MemoryPackOrder(12)]
    public int PortQuery { get; set; } = 0;
    [MemoryPackOrder(13)]
    public int PortRcon { get; set; }
    [MemoryPackOrder(14)]
    public bool Modded { get; set; }
    [MemoryPackOrder(15)]
    public string ManualRootUrl { get; set; } = "";
    [MemoryPackOrder(16)]
    public string ServerProcessName { get; set; } = "";
    [MemoryPackOrder(17)]
    public ConnectivityState ServerState { get; set; }
    [MemoryPackOrder(18)]
    public GameSource Source { get; set; }
    [MemoryPackOrder(19)]
    public SerializableList<ModSlim> ModList { get; set; } = new();
    [MemoryPackOrder(20)]
    public SerializableList<LocalResourceSlim> Resources { get; set; } = new();
}