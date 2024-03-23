using Domain.Enums;
using MemoryPack;

namespace Domain.Models.ControlServer;

[MemoryPackable]
public partial class HostWork
{
    public HostWorkType Type { get; set; }
    public List<string> Messages { get; set; } = new();
}