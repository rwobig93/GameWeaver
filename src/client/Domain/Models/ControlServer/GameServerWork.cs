using Domain.Enums;
using MemoryPack;

namespace Domain.Models.ControlServer;

[MemoryPackable]
public class GameServerWork
{
    public GameServerWorkType Type { get; set; }
    public List<string> Messages { get; set; } = new();
    public ServerState ServerState { get; set; } = ServerState.Unknown;
    public string ServerVersion { get; set; } = "";
}