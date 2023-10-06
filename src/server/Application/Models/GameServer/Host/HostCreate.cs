using Domain.Enums.GameServer;

namespace Application.Models.GameServer.Host;

public class HostCreate
{
    public Guid OwnerId { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;
    public string Hostname { get; set; } = "";
    public string FriendlyName { get; set; } = "";
    public string PrivateIp { get; set; } = "";
    public string PublicIp { get; set; } = "";
    public ConnectivityState CurrentState { get; set; } = ConnectivityState.Unknown;
    public OsType Os { get; set; }
    public string AllowedPorts { get; set; } = "";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public Guid? LastModifiedBy { get; set; } = null;
    public DateTime? LastModifiedOn { get; set; } = null;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedOn { get; set; } = null;
}