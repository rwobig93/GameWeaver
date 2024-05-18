using System.Net;
using Domain.Contracts;
using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable]
public partial class HostNetworkInterface
{
    public string Name { get; set; } = null!;

    public ulong Speed { get; set; }

    public string Type { get; set; } = null!;
    
    public string TypeDetail { get; set; } = null!;

    public string MacAddress { get; set; } = null!;

    public SerializableList<IPAddress> IpAddresses { get; set; } = [];
    
    public SerializableList<IPAddress> DefaultGateways { get; set; } = [];

    [MemoryPackAllowSerialize]
    public IPAddress DhcpServer { get; set; } = null!;

    public SerializableList<IPAddress> DnsServers { get; set; } = [];
}