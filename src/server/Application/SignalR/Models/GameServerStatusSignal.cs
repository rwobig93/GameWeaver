using Domain.Enums.GameServer;

namespace Application.SignalR.Models;

public class GameServerStatusSignal
{
    public Guid Id { get; set; }
    public string ServerName { get; set; }
    public ConnectivityState ServerState { get; set; }
}