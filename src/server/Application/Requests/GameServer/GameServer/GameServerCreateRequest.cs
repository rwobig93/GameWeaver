namespace Application.Requests.GameServer.GameServer;

public class GameServerCreateRequest
{
    public Guid OwnerId { get; set; }
    public Guid HostId { get; set; }
    public Guid GameId { get; set; }
    public Guid? ParentGameProfileId { get; set; }
    public string Name { get; set; } = null!;
    public string Password { get; set; } = "";
    public string PasswordRcon { get; set; } = "";
    public string PasswordAdmin { get; set; } = "";
    public string ExternalUrl { get; set; } = "";
    public int PortGame { get; set; } = 0;
    public int PortQuery { get; set; } = 0;
    public int PortRcon { get; set; } = 0;
    public bool Modded { get; set; } = false;
    public bool Private { get; set; } = true;
}