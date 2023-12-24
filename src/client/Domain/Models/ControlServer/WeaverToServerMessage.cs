using Domain.Enums;

namespace Domain.Models.ControlServer;

public class WeaverToServerMessage
{
    // TODO: Update response to be proper for the control server and handle the response on the server side
    public int Id { get; set; }
    public Guid? GameServerId { get; set; }
    public WeaverWorkTarget TargetType { get; set; }
    public WeaverWorkState Status { get; set; }
    public string WorkData { get; set; } = "";
    public int AttemptCount { get; set; }
}