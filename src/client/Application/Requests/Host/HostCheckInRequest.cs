namespace Application.Requests.Host;

public class HostCheckInRequest
{
    public DateTime SendTimestamp { get; set; }
    public float CpuUsage { get; set; }
    public float RamUsage { get; set; }
    public float Uptime { get; set; }
    public int NetworkOutMb { get; set; }
    public int NetworkInMb { get; set; }
}