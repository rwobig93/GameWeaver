using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable]
public partial class HostOperatingSystem
{
    public string Name { get; set; } = null!;

    public string Version { get; set; } = null!;
}