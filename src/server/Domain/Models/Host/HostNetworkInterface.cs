using Domain.Contracts;
using MemoryPack;

namespace Domain.Models.Host;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class HostNetworkInterface
{
    [MemoryPackOrder(0)]
    public string Name { get; set; } = null!;
    [MemoryPackOrder(1)]
    public ulong Speed { get; set; }
    [MemoryPackOrder(2)]
    public string Type { get; set; } = null!;
    [MemoryPackOrder(3)]
    public string TypeDetail { get; set; } = null!;
    [MemoryPackOrder(4)]
    public string MacAddress { get; set; } = null!;
    [MemoryPackOrder(5)]
    public SerializableList<string> IpAddresses { get; set; } = [];
    [MemoryPackOrder(6)]
    public SerializableList<string> DefaultGateways { get; set; } = [];
    [MemoryPackOrder(7)]
    public string DhcpServer { get; set; } = null!;
    [MemoryPackOrder(8)]
    public SerializableList<string> DnsServers { get; set; } = [];
}