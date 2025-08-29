namespace Application.Models.GameServer.GameServer;

public class GameServerParentUpdate
{
    public Guid Id { get; set; }
    public Guid? ParentGameProfileId { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}