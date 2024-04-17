using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable]
public partial class HostRam
{
    public string Manufacturer { get; set; } = null!;

    public ulong Speed { get; set; }

    public ulong Capacity { get; set; }
}