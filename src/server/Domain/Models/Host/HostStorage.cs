using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class HostStorage
{
    [MemoryPackOrder(0)]
    public uint Index { get; set; }
    [MemoryPackOrder(1)]
    public string Model { get; set; } = null!;
    [MemoryPackOrder(2)]
    public string MountPoint { get; set; } = null!;
    [MemoryPackOrder(3)]
    public string Name { get; set; } = null!;
    [MemoryPackOrder(4)]
    public ulong TotalSpace { get; set; }
    [MemoryPackOrder(5)]
    public ulong FreeSpace { get; set; }
}