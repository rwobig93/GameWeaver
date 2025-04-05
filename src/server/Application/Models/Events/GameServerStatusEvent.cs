using Domain.Enums.GameServer;

namespace Application.Models.Events;

public class GameServerStatusEvent : EventArgs
{
    public Guid Id { get; set; }
    public string ServerName { get; set; } = "";
    public bool BuildVersionUpdated = false;
    public ConnectivityState? ServerState { get; set; }
    public string RunningConfigHash { get; set; } = "";
    public string StorageConfigHash { get; set; } = "";
}