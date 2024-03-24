namespace Application.Requests.Host;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class HostCheckInRequest
{
    [MemoryPackOrder(0)]
    public DateTime SendTimestamp { get; set; }
    [MemoryPackOrder(1)]
    public double CpuUsage { get; set; }
    [MemoryPackOrder(2)]
    public double RamUsage { get; set; }
    [MemoryPackOrder(3)]
    public long Uptime { get; set; }
    [MemoryPackOrder(4)]
    public double NetworkOutMb { get; set; }
    [MemoryPackOrder(5)]
    public double NetworkInMb { get; set; }
}