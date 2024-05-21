using Domain.Enums.GameServer;
using Domain.Models.Host;

namespace Application.Models.GameServer.Host;

public class HostSlim
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Hostname { get; set; } = "";
    public string FriendlyName { get; set; } = "";
    public string Description { get; set; } = "";
    public string PrivateIp { get; set; } = "";
    public string PublicIp { get; set; } = "";
    public ConnectivityState CurrentState { get; set; }
    public OsType Os { get; set; }
    public string OsName { get; set; } = "";
    public string OsVersion { get; set; } = "";
    public List<string> AllowedPorts { get; set; } = [];
    public List<HostCpu> Cpus { get; set; } = [];
    public List<HostMotherboard> Motherboards { get; set; } = [];
    public List<HostStorage> Storage { get; set; } = [];
    public List<HostNetworkInterface> NetworkInterfaces { get; set; } = [];
    public List<HostRam> RamModules { get; set; } = [];
    public List<HostGpu> Gpus { get; set; } = [];
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}