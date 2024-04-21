using Domain.Enums;
using MemoryPack;

namespace Domain.Models.GameServer;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class SoftwareUpdateStatus
{
    [MemoryPackOrder(0)]
    public string Name { get; set; } = "";
    [MemoryPackOrder(1)]
    public int SteamId { get; set; }
    [MemoryPackOrder(2)]
    public string UpdateUrl { get; set; } = "";
    [MemoryPackOrder(3)]
    public string CurrentVersion { get; set; } = "";
    [MemoryPackOrder(4)]
    public string NewVersion { get; set; } = "";
    [MemoryPackIgnore]
    public bool UpdateAvailable => CurrentVersion != NewVersion;
    [MemoryPackOrder(5)]
    public SoftwareType Type { get; set; }
    [MemoryPackOrder(6)]
    public GameSource Source { get; set; }
}