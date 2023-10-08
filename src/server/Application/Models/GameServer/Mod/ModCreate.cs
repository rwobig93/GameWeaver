namespace Application.Models.GameServer.Mod;

public class ModCreate
{
    public Guid GameId { get; set; }
    public int SteamGameId { get; set; }
    public int SteamToolId { get; set; }
    public string SteamId { get; set; } = "";
    public string FriendlyName { get; set; } = "";
    public string CurrentHash { get; set; } = "";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}