namespace Domain.Enums;

public enum ServerState
{
    Connectable,
    InternallyConnectable,
    Shutdown,
    Updating,
    Unknown,
    Stalled,
    Unreachable,
    SpinningUp,
    Restarting,
    Installing
}