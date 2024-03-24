using Domain.Contracts;
using Domain.Enums;
using MemoryPack;

namespace Domain.Models.GameServer;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class LocationPointer
{
    [MemoryPackOrder(0)]
    public string Name { get; set; } = "";
    [MemoryPackOrder(1)]
    public string Path { get; set; } = "";
    [MemoryPackOrder(2)]
    public bool Startup { get; set; } = false;
    [MemoryPackOrder(3)]
    public LocationType Type { get; set; }
    [MemoryPackOrder(4)]
    public string Extension { get; set; } = "";
    [MemoryPackOrder(5)]
    public string Args { get; set; } = "";
    [MemoryPackOrder(6)]
    public SerializableList<ConfigurationSet> ConfigSets { get; set; } = new();
}