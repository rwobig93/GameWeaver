using Domain.Enums.GameServer;

namespace Application.Models.GameServer.Host;

public class HostUpdate
{
    public Guid Id { get; set; }
    public Guid? OwnerId { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }
    public string? Hostname { get; set; }
    public string? FriendlyName { get; set; }
    public string? PrivateIp { get; set; }
    public string? PublicIp { get; set; }
    public ConnectivityState? CurrentState { get; set; }
    public OsType? Os { get; set; }
    public string? AllowedPorts { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}