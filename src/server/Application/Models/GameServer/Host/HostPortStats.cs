namespace Application.Models.GameServer.Host;

public class HostPortStats
{
    public int TotalPorts { get; set; }
    public int AvailablePorts { get; set; }
    public int UsedPorts { get; set; }
    public int UsedGameserversWorth { get; set; }
    public int AvailableGameserversWorth { get; set; }
}