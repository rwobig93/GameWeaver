using Domain.Enums.GameServer;

namespace Application.Models.GameServer.Host;

public class HostCreateDb
{
    public Guid OwnerId { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;
    public string Hostname { get; set; } = "";
    public string FriendlyName { get; set; } = "";
    public string Description { get; set; } = "";
    public string PrivateIp { get; set; } = "";
    public string PublicIp { get; set; } = "";
    public ConnectivityState CurrentState { get; set; } = ConnectivityState.Unknown;
    public OsType Os { get; set; }
    public string OsName { get; set; } = "";
    public string OsVersion { get; set; } = "";
    public byte[] AllowedPorts { get; set; } = [];
    public byte[] Cpus { get; set; } = [];
    public byte[] Motherboards { get; set; } = [];
    public byte[] Storage { get; set; } = [];
    public byte[] NetworkInterfaces { get; set; } = [];
    public byte[] RamModules { get; set; } = [];
    public byte[] Gpus { get; set; } = [];
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}