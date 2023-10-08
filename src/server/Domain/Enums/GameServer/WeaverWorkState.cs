namespace Domain.Enums.GameServer;

public enum WeaverWorkState
{
    WaitingToBePickedUp = 0,
    PickedUp = 1,
    Completed = 2,
    Cancelled = 3,
    Failed = 4
}