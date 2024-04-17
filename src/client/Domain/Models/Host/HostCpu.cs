using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable]
public partial class HostCpu
{
    public string Name { get; set; } = null!;

    public int CoreCount { get; set; }

    public int LogicalProcessorCount { get; set; }

    public string SocketDesignation { get; set; } = null!;
}