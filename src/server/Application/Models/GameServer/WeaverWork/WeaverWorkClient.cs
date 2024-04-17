using Domain.Enums.GameServer;
using MemoryPack;

namespace Application.Models.GameServer.WeaverWork;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class WeaverWorkClient
{
    [MemoryPackOrder(0)]
    public int Id { get; set; }
    [MemoryPackOrder(1)]
    public Guid? GameServerId { get; set; }
    [MemoryPackOrder(2)]
    public WeaverWorkTarget TargetType { get; set; }
    [MemoryPackOrder(3)]
    public WeaverWorkState Status { get; set; }
    [MemoryPackOrder(4)]
    public byte[]? WorkData { get; set; }
}