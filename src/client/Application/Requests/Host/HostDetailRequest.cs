using Domain.Contracts;
using Domain.Models.Host;

namespace Application.Requests.Host;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class HostDetailRequest
{
    [MemoryPackOrder(0)]
    public SerializableList<HostCpu> Cpus { get; set; } = [];

    [MemoryPackOrder(1)]
    public SerializableList<HostMotherboard> Motherboards { get; set; } = [];

    [MemoryPackOrder(2)]
    public SerializableList<HostStorage> Storage { get; set; } = [];

    [MemoryPackOrder(3)]
    public SerializableList<HostNetworkInterface> NetworkInterfaces { get; set; } = [];

    [MemoryPackOrder(4)]
    public SerializableList<HostRam> RamModules { get; set; } = [];

    [MemoryPackOrder(5)]
    public HostOperatingSystem Os { get; set; } = new();

    [MemoryPackOrder(6)]
    public SerializableList<HostGpu> Gpus { get; set; } = [];
}