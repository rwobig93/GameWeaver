using Domain.Enums.GameServer;
using MemoryPack;

namespace Application.Models.GameServer.WeaverWork;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class WeaverWorkSlim
{
    [MemoryPackOrder(0)]
    public int Id { get; set; }
    [MemoryPackOrder(1)]
    public Guid HostId { get; set; }
    [MemoryPackOrder(2)]
    public Guid? GameServerId { get; set; }
    [MemoryPackAllowSerialize]
    [MemoryPackOrder(3)]
    public WeaverWorkTarget TargetType { get; set; }
    [MemoryPackAllowSerialize]
    [MemoryPackOrder(4)]
    public WeaverWorkState Status { get; set; } = WeaverWorkState.WaitingToBePickedUp;
    [MemoryPackOrder(5)]
    public byte[]? WorkData { get; set; }
    [MemoryPackOrder(6)]
    public Guid CreatedBy { get; set; }
    [MemoryPackOrder(7)]
    public DateTime CreatedOn { get; set; }
    [MemoryPackOrder(8)]
    public Guid? LastModifiedBy { get; set; }
    [MemoryPackOrder(9)]
    public DateTime? LastModifiedOn { get; set; }
}