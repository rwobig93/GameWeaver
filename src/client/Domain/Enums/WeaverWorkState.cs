namespace Domain.Enums;

public enum WeaverWorkState
{
    WaitingToBePickedUp = 0,
    PickedUp = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Failed = 5
}