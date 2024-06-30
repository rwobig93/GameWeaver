using Application.Models.GameServer.ConfigurationItem;
using Domain.Contracts;
using Domain.Enums.GameServer;
using MemoryPack;

namespace Application.Models.GameServer.LocalResource;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class LocalResourceHost
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
    public int StartupPriority { get; set; }
    [MemoryPackOrder(5)]
    public ResourceType Type { get; set; }
    [MemoryPackOrder(6)]
    public ContentType ContentType { get; set; }
    [MemoryPackOrder(7)]
    public string Args { get; set; } = "";
    [MemoryPackOrder(8)]
    public SerializableList<ConfigurationItemHost> ConfigSets { get; set; } = [];
    [MemoryPackOrder(9)]
    public Guid Id { get; set; }
    [MemoryPackOrder(10)]
    public bool LoadExisting { get; set; }
}