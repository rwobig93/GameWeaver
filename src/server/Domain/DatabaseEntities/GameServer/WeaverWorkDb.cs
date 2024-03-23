using Domain.Contracts;
using Domain.Enums.GameServer;

namespace Domain.DatabaseEntities.GameServer;

public class WeaverWorkDb : IAuditableEntity<int>
{
    public int Id { get; set; }
    public Guid HostId { get; set; }
    public WeaverWorkTarget TargetType { get; set; }
    public WeaverWorkState Status { get; set; } = WeaverWorkState.WaitingToBePickedUp;
    public byte[]? WorkData { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}