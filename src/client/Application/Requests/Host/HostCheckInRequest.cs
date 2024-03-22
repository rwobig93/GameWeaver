namespace Application.Requests.Host;

[MemoryPackable]
public partial class HostCheckInRequest
{
    public DateTime SendTimestamp { get; set; }
    public double CpuUsage { get; set; }
    public double RamUsage { get; set; }
    public long Uptime { get; set; }
    public double NetworkOutMb { get; set; }
    public double NetworkInMb { get; set; }
}