using Domain.Enums;
using MemoryPack;

namespace Domain.Models.ControlServer;

[MemoryPackable]
public class GameServerWork
{
    public GameServerWorkType Type { get; set; }
    public List<string> Messages { get; set; } = new();
}