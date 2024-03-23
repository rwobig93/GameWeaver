using Domain.Contracts;
using Domain.Models.Host;

namespace Application.Requests.Host;

[MemoryPackable]
public partial class HostDetailRequest
{
    public SerializableList<HostCpu> Cpus { get; set; } = new();

    public SerializableList<HostMotherboard> Motherboards { get; set; } = new();

    public SerializableList<HostStorage> Storage { get; set; } = new();

    public SerializableList<HostNetworkInterface> NetworkInterfaces { get; set; } = new();

    public SerializableList<HostRam> RamModules { get; set; } = new();

    public HostOperatingSystem Os { get; set; } = new();

    public SerializableList<HostGpu> Gpus { get; set; } = new();
}