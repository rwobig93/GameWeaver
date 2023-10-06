namespace Domain.Enums.GameServer;

public enum ConnectivityState
{
    Unknown = 0,
    Shutdown = 1,
    SpinningUp = 2,
    Connectable = 3,
    InternallyConnectable = 4,
    Updating = 5,
    Stalled = 6,
    Unreachable = 7,
    Restarting = 8,
    Installing = 9
}