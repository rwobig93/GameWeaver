using Domain.Enums;

namespace Domain.Models.ControlServer;

public class WeaverToClientMessage
{
    public Guid WorkId { get; set; }
    public WeaverMessageAction Action { get; set; }
    public WeaverResourceType Resource { get; set; }
    public Guid ResourceId { get; set; }
    public string Command { get; set; } = "";
    public string CommandContext { get; set; } = "";
}