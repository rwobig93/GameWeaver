using System.Diagnostics;
using Domain.Models.GameServer;

namespace Domain.Models.Host;

public class RunSteamDto
{
    public string Command { get; set; } = "";
    public DataReceivedEventHandler? OutputHandler { get; set; }
    public EventHandler? ExitHandler { get; set; }
    public GameServerLocal? GameServer { get; set; }
    public bool CheckForUpdate { get; set; } = false;
}
