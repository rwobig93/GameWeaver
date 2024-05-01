namespace Domain.Models.Network;

public class ProcessNetworkInfo
{
    public int ProcessId { get; set; }
    public string Protocol { get; set; } = "";
    public string Port { get; set; } = "";
}