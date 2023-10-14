using Domain.Enums;

namespace Domain.Models;

public class WeaverToServerMessage
{
    public WeaverMessageAction Action { get; set; }
    public int AttemptCount { get; set; } = 0;
}