using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class HostMotherboard
{
    [MemoryPackOrder(0)]
    public string Product { get; set; } = null!;
    [MemoryPackOrder(1)]
    public string Manufacturer { get; set; } = null!;
}