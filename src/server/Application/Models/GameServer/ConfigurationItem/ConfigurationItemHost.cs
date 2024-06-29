using MemoryPack;

namespace Application.Models.GameServer.ConfigurationItem;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class ConfigurationItemHost
{
    [MemoryPackOrder(0)]
    public Guid Id { get; set; }
    [MemoryPackOrder(1)]
    public bool DuplicateKey { get; set; }
    [MemoryPackOrder(2)]
    public string Category { get; set; } = null!;
    [MemoryPackOrder(3)]
    public string Key { get; set; } = null!;
    [MemoryPackOrder(4)]
    public string Value { get; set; } = null!;
    [MemoryPackOrder(5)]
    public string Path { get; set; } = null!;
}