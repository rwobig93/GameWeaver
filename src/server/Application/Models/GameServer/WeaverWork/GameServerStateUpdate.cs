using Application.Models.GameServer.LocalResource;
using Domain.Contracts;
using Domain.Enums.GameServer;
using MemoryPack;

namespace Application.Models.GameServer.WeaverWork;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class GameServerStateUpdate
{
    [MemoryPackOrder(0)] public Guid Id { get; set; }
    [MemoryPackOrder(1)] public bool BuildVersionUpdated { get; set; }
    [MemoryPackAllowSerialize]
    [MemoryPackOrder(2)] public ConnectivityState? ServerState { get; set; }
    [MemoryPackOrder(3)] public SerializableList<LocalResourceSlim>? Resources { get; set; }
    [MemoryPackOrder(4)] public string? RunningConfigHash { get; set; }
    [MemoryPackOrder(5)] public string? StorageConfigHash { get; set; }
    [MemoryPackOrder(6)] public List<string>? Messages { get; set; }
}