using Domain.Contracts;
using Domain.Enums;
using Domain.Models.GameServer;
using MemoryPack;

namespace Domain.Models.ControlServer;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class GameServerStateUpdate
{
    [MemoryPackOrder(0)] public Guid Id { get; set; }
    [MemoryPackOrder(1)] public bool BuildVersionUpdated { get; set; }
    [MemoryPackOrder(2)] public ServerState? ServerState { get; set; }
    [MemoryPackOrder(3)] public SerializableList<LocalResource>? Resources { get; set; }
    [MemoryPackOrder(4)] public string? RunningConfigHash { get; set; }
    [MemoryPackOrder(5)] public string? StorageConfigHash { get; set; }
    [MemoryPackOrder(6)] public List<string>? Messages { get; set; }
}