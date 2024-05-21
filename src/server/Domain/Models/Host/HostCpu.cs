using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class HostCpu
{
    [MemoryPackOrder(0)]
    public string Name { get; set; } = null!;
    [MemoryPackOrder(1)]
    public int CoreCount { get; set; }
    [MemoryPackOrder(2)]
    public int LogicalProcessorCount { get; set; }
    [MemoryPackOrder(3)]
    public string SocketDesignation { get; set; } = null!;
}