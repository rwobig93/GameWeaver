using Domain.Models.Host;

namespace Application.Requests.Host;

[MemoryPackable]
public partial class HostDetailRequest
{
    public List<HostCpu> Cpus { get; set; } = new();

    public List<HostMotherboard> Motherboards { get; set; } = new();

    public List<HostStorage> Storage { get; set; } = new();

    public List<HostNetworkInterface> NetworkInterfaces { get; set; } = new();

    public List<HostRam> RamModules { get; set; } = new();

    public HostOperatingSystem Os { get; set; } = new();

    public List<HostGpu> Gpus { get; set; } = new();
}