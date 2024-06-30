namespace Application.Models.GameServer.HostCheckIn;

public class HostCheckInFull
{
    public int Id { get; set; }
    public Guid HostId { get; set; }
    public DateTime SendTimestamp { get; set; }
    public DateTime ReceiveTimestamp { get; set; }
    public float CpuUsage { get; set; }
    public float RamUsage { get; set; }
    public float Uptime { get; set; }
    public int NetworkOutBytes { get; set; }
    public int NetworkInBytes { get; set; }
}