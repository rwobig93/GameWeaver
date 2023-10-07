using Domain.Contracts;

namespace Domain.DatabaseEntities.GameServer;

public class GameProfileDb : IAuditableEntity<Guid>
{
    public Guid Id { get; set; }
    public string FriendlyName { get; set; } = "";
    public Guid OwnerId { get; set; }
    public Guid GameId { get; set; }
    public string ServerProcessName { get; set; } = "";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}