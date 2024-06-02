using MemoryPack;

namespace Application.Requests.GameServer.Host;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class HostCheckInRequest
{
    [MemoryPackOrder(0)]
    public DateTime SendTimestamp { get; set; }
    [MemoryPackOrder(1)]
    public float CpuUsage { get; set; }
    [MemoryPackOrder(2)]
    public float RamUsage { get; set; }
    [MemoryPackOrder(3)]
    public float Uptime { get; set; }
    [MemoryPackOrder(4)]
    public int NetworkOutBytes { get; set; }
    [MemoryPackOrder(5)]
    public int NetworkInBytes { get; set; }
}