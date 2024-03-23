using Domain.Enums.GameServer;
using Domain.Enums.WeaverWork;
using MemoryPack;
using WeaverWorkTarget = Domain.Enums.WeaverWork.WeaverWorkTarget;

namespace Application.Models.GameServer.WeaverWork;

[MemoryPackable]
public partial class GameServerWork
{
    public WeaverWorkTarget Target { get; set; }
    public List<string> Messages { get; set; } = new();
    public ConnectivityState ServerState { get; set; }
    public string ServerVersion { get; set; } = "";
}