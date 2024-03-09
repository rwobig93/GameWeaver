namespace Domain.Models.Host;

public class HostRam
{
    public string Manufacturer { get; set; } = null!;

    public ulong Speed { get; set; }

    public ulong Capacity { get; set; }
}