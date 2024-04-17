namespace Domain.Models.Host;

public class HostResourceUsage
{
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public double CpuUsage { get; set; }
    public double RamUsage { get; set; }
    public long Uptime { get; set; }
    public double NetworkOutBytes { get; set; }
    public double NetworkInBytes { get; set; }
}