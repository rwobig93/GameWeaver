using GameServerQuery.Steam.Enums;

namespace GameServerQuery.Steam.Models;

public class SteamServerInfo
{
    public int Latency { get; set; }
    public byte Protocol { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Map { get; set; } = string.Empty;
    public string Folder { get; set; } = string.Empty;
    public string Game { get; set; } = string.Empty;
    public ushort Id { get; set; }
    public int Players { get; set; }
    public int MaxPlayers { get; set; }
    public int Bots { get; set; }
    public SteamServerType ServerType { get; set; }
    public SteamServerEnvironment Environment { get; set; }
    public bool HasPassword { get; set; }
    public bool VacEnabled { get; set; }
    public SteamTheShipMode TheShipMode { get; set; }
    public string Version { get; set; } = string.Empty;
    public byte ExtraDataFlag { get; set; }
    public ushort Port { get; set; }
    public ulong SteamId { get; set; }
    public ushort SourceTvPort { get; set; }
    public string SourceTvName { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public ulong GameId { get; set; }
    public byte TheShipWitnesses { get; set; }
    public byte TheShipDuration { get; set; }
}