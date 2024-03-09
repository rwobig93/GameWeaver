namespace Domain.Models.Host;

public class HostGpu
{
    public string Name { get; set; } = null!;

    public string DriverVersion { get; set; } = null!;

    public string VideoMode { get; set; } = null!;

    public ulong AdapterRam { get; set; }
}