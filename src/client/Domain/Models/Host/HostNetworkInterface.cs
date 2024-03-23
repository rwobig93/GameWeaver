﻿using System.Net;
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

    public List<IPAddress> IpAddresses { get; set; } = new();
    
    public List<IPAddress> DefaultGateways { get; set; } = new();

    [MemoryPackAllowSerialize]
    public IPAddress DhcpServer { get; set; } = null!;

    public List<IPAddress> DnsServers { get; set; } = new();
}