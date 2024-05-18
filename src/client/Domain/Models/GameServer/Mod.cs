using MemoryPack;

namespace Domain.Models.GameServer;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class Mod
{
    [MemoryPackOrder(0)]
    public int GameId { get; set; }
    [MemoryPackOrder(1)]
    public int ToolId { get; set; }
    [MemoryPackOrder(2)]
    public string SteamId { get; set; } = "";
    [MemoryPackOrder(3)]
    public string FriendlyName { get; set; } = "";
}