using MemoryPack;

namespace Application.Requests.v1.GameServer;

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
    public int NetworkOutMb { get; set; }
    [MemoryPackOrder(5)]
    public int NetworkInMb { get; set; }
}