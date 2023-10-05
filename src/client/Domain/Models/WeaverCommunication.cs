using Domain.Enums;

namespace Domain.Models;

public class WeaverCommunication
{
    public WeaverCommAction Action { get; set; }
    public int AttemptCount { get; set; } = 0;
}