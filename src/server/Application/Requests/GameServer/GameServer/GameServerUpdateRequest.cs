namespace Application.Requests.GameServer.GameServer;

public class GameServerUpdateRequest
{
    public Guid Id { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? ParentGameProfileId { get; set; }
    public string? ServerBuildVersion { get; set; }
    public string? ServerName { get; set; }
    public string? Password { get; set; }
    public string? PasswordRcon { get; set; }
    public string? PasswordAdmin { get; set; }
    public int? PortGame { get; set; }
    public int? PortPeer { get; set; }
    public int? PortQuery { get; set; }
    public int? PortRcon { get; set; }
    public bool? Modded { get; set; }
    public bool? Private { get; set; }
}