using Application.Models.GameServer.ConfigurationItem;
using Domain.Contracts;
using Domain.Enums.GameServer;
using MemoryPack;

namespace Application.Models.GameServer.LocalResource;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class LocalResourceHost
{
    [MemoryPackOrder(0)]
    public string Name { get; set; } = "";
    [MemoryPackOrder(1)]
    public string Path { get; set; } = "";
    [MemoryPackOrder(2)]
    public bool Startup { get; set; } = false;
    [MemoryPackOrder(3)]
    public ResourceType Type { get; set; }
    [MemoryPackOrder(4)]
    public string Extension { get; set; } = "";
    [MemoryPackOrder(5)]
    public string Args { get; set; } = "";
    [MemoryPackOrder(6)]
    public SerializableList<ConfigurationItemSlim> ConfigSets { get; set; } = [];
}