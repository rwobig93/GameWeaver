using Domain.Enums;
using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class HostOperatingSystem
{
    [MemoryPackOrder(0)]
    public string Name { get; set; } = null!;
    [MemoryPackOrder(1)]
    public string Version { get; set; } = null!;
    [MemoryPackOrder(2)]
    public OsType Os { get; set; } = OsType.Unknown;
}