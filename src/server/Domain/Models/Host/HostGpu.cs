using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class HostGpu
{
    [MemoryPackOrder(0)]
    public string Name { get; set; } = null!;
    [MemoryPackOrder(1)]
    public string DriverVersion { get; set; } = null!;
    [MemoryPackOrder(2)]
    public string VideoMode { get; set; } = null!;
    [MemoryPackOrder(3)]
    public ulong AdapterRam { get; set; }
}