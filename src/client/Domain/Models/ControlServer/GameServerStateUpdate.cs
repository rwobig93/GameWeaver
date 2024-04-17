using Domain.Contracts;
using Domain.Enums;
using Domain.Models.GameServer;
using MemoryPack;

namespace Domain.Models.ControlServer;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class GameServerStateUpdate
{
    [MemoryPackOrder(0)] public Guid Id { get; set; }
    [MemoryPackOrder(1)] public string? ServerProcessName { get; set; }
    [MemoryPackOrder(2)] public ServerState ServerState { get; set; } = ServerState.Unknown;
    [MemoryPackOrder(3)] public SerializableList<LocationPointer>? Resources { get; set; }

}