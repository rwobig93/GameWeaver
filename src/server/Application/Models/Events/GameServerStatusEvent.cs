using Domain.Enums.GameServer;

namespace Application.Models.Events;

public class GameServerStatusEvent : EventArgs
{
    public Guid Id { get; set; }
    public string ServerName { get; set; } = "";
    public ConnectivityState ServerState { get; set; }
}