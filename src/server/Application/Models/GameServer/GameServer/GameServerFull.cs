using Domain.Enums.GameServer;

namespace Application.Models.GameServer.GameServer;

public class GameServerFull
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Guid HostId { get; set; }
    public Guid GameId { get; set; }
    public Guid GameProfileId { get; set; }
    public Guid? ParentGameProfileId { get; set; }
    public string ServerBuildVersion { get; set; } = "";
    public string ServerName { get; set; } = "";
    public string Password { get; set; } = "";
    public string PasswordRcon { get; set; } = "";
    public string PasswordAdmin { get; set; } = "";
    public string PublicIp { get; set; } = "";
    public string PrivateIp { get; set; } = "";
    public string ExternalHostname { get; set; } = "";
    public int PortGame { get; set; }
    public int PortQuery { get; set; }
    public int PortRcon { get; set; }
    public bool Modded { get; set; }
    public bool Private { get; set; }
    public ConnectivityState ServerState { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}