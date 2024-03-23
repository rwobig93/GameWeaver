using Domain.Enums;
using MemoryPack;

namespace Domain.Models.ControlServer;

[MemoryPackable]
public partial class WeaverWorkClient
{
    public int Id { get; set; }
    public Guid? GameServerId { get; set; }
    public WeaverWorkTarget TargetType { get; set; }
    public WeaverWorkState Status { get; set; }
    public byte[]? WorkData { get; set; }
}