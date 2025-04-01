namespace Application.Models.GameServer.GameProfile;

public class GameProfileCreate
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string FriendlyName { get; set; } = "";
    public Guid OwnerId { get; set; }
    public Guid GameId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}