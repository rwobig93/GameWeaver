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
    [MemoryPackOrder(2)] public ConnectivityState ServerState { get; set; } = ConnectivityState.Unknown;
    [MemoryPackOrder(3)] public SerializableList<LocalResourceSlim>? Resources { get; set; }
}