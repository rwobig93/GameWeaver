using Domain.Contracts;
using Domain.Enums;
using MemoryPack;

namespace Domain.Models.GameServer;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class LocationPointer
{
    [MemoryPackOrder(0)]
    public Guid GameserverId { get; set; }
    [MemoryPackOrder(1)]
    public string Name { get; set; } = "";
    [MemoryPackOrder(2)]
    public string Path { get; set; } = "";
    [MemoryPackOrder(3)]
    public bool Startup { get; set; }
    [MemoryPackOrder(4)]
    public LocationType Type { get; set; }
    [MemoryPackOrder(5)]
    public string Extension { get; set; } = "";
    [MemoryPackOrder(6)]
    public string Args { get; set; } = "";
    [MemoryPackOrder(7)]
    public SerializableList<ConfigurationSet> ConfigSets { get; set; } = [];
}