using Domain.Contracts;
using Domain.Models.Host;
using MemoryPack;

namespace Application.Models.GameServer.WeaverWork;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class HostDetailUpdate
{
    [MemoryPackOrder(0)]
    public Guid HostId { get; set; }

    [MemoryPackOrder(1)]
    public string Hostname { get; set; } = "";
    [MemoryPackAllowSerialize]

    [MemoryPackOrder(2)]
    public SerializableList<HostCpu> Cpus { get; set; } = [];
    [MemoryPackAllowSerialize]

    [MemoryPackOrder(3)]
    public SerializableList<HostMotherboard> Motherboards { get; set; } = [];
    [MemoryPackAllowSerialize]

    [MemoryPackOrder(4)]
    public SerializableList<HostStorage> Storage { get; set; } = [];
    [MemoryPackAllowSerialize]

    [MemoryPackOrder(5)]
    public SerializableList<HostNetworkInterface> NetworkInterfaces { get; set; } = [];
    [MemoryPackAllowSerialize]

    [MemoryPackOrder(6)]
    public SerializableList<HostRam> RamModules { get; set; } = [];
    [MemoryPackAllowSerialize]

    [MemoryPackOrder(7)]
    public HostOperatingSystem Os { get; set; } = new();
    [MemoryPackAllowSerialize]

    [MemoryPackOrder(8)]
    public SerializableList<HostGpu> Gpus { get; set; } = [];
}