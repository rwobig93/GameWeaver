using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable]
public partial class HostMotherboard
{
    public string Product { get; set; } = null!;

    public string Manufacturer { get; set; } = null!;
}