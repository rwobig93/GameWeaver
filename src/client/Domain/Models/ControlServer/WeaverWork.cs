using Domain.Enums;
using MemoryPack;

namespace Domain.Models.ControlServer;

[MemoryPackable(SerializeLayout.Explicit)]
public partial class WeaverWork
{
    // TODO: Add pack order to each [MemoryPackable] object since order makes a difference, best to be explicit in this case
    [MemoryPackOrder(0)]
    public int Id { get; set; }
    [MemoryPackOrder(1)]
    public WeaverWorkTarget Type { get; set; }
    [MemoryPackOrder(2)]
    public WeaverWorkState Status { get; set; }
    [MemoryPackOrder(3)]
    public byte[]? WorkData { get; set; }
}