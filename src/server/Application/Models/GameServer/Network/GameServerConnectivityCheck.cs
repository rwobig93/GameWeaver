using Domain.Enums.GameServer;

namespace Application.Models.GameServer.Network;

public class GameServerConnectivityCheck
{
    public string HostIp { get; set; } = "";
    public int PortGame { get; set; }
    public int PortQuery { get; set; }
    public NetworkProtocol Protocol { get; set; }
    public int TimeoutMilliseconds { get; set; } = 1000;
    public GameSource Source { get; set; }
}