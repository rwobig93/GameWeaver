namespace Domain.Enums;

public enum ServerState
{
    Unknown = 0,
    UnRegistered = 1,
    SpinningUp = 2,
    Connectable = 3,
    InternallyConnectable = 4,
    Updating = 5,
    Stalled = 6,
    Unreachable = 7,
    Restarting = 8,
    Installing = 9,
    Shutdown = 10,
    Uninstalling = 11,
    Uninstalled = 12,
    ShuttingDown = 13,
    OverlappingPort = 14,
    Discovering = 15
}