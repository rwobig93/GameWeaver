using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class HostRam
{
    [MemoryPackOrder(0)]
    public string Manufacturer { get; set; } = null!;
    [MemoryPackOrder(1)]
    public ulong Speed { get; set; }
    [MemoryPackOrder(2)]
    public ulong Capacity { get; set; }
}