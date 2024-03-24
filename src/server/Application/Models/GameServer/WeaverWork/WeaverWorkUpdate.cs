using Domain.Enums.GameServer;
using MemoryPack;

namespace Application.Models.GameServer.WeaverWork;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class WeaverWorkUpdate
{
    [MemoryPackOrder(0)]
    public int Id { get; set; }
    [MemoryPackOrder(1)]
    public Guid? HostId { get; set; }
    [MemoryPackOrder(2)]
    public WeaverWorkTarget? TargetType { get; set; }
    [MemoryPackOrder(3)]
    public WeaverWorkState? Status { get; set; }
    [MemoryPackOrder(4)]
    public byte[]? WorkData { get; set; }
    [MemoryPackOrder(5)]
    public Guid? CreatedBy { get; set; }
    [MemoryPackOrder(6)]
    public DateTime? CreatedOn { get; set; }
    [MemoryPackOrder(7)]
    public Guid? LastModifiedBy { get; set; }
    [MemoryPackOrder(8)]
    public DateTime? LastModifiedOn { get; set; }
}