using Domain.Enums;

namespace Domain.Models.ControlServer;

public class WeaverToServerMessage
{
    public Guid WorkId { get; set; }
    public HostWorkStatus Status { get; set; }
    public string StatusContext { get; set; } = "";
    public int AttemptCount { get; set; }
}