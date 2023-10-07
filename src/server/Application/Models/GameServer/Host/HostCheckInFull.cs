namespace Application.Models.GameServer.Host;

public class HostCheckInFull
{
    public int Id { get; set; }
    public Guid HostId { get; set; }
    public DateTime SendTimestamp { get; set; }
    public DateTime ReceiveTimestamp { get; set; }
    public float CpuUsage { get; set; }
    public float RamUsage { get; set; }
    public float Uptime { get; set; }
    public int NetworkOutMb { get; set; }
    public int NetworkInMb { get; set; }
}