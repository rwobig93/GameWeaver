using Domain.Enums.GameServer;

namespace Application.Models.GameServer.LocalResource;

public class LocalResourceCreate
{
    public Guid GameProfileId { get; set; }
    public Guid GameServerId { get; set; }
    public string Name { get; set; } = "";
    public string PathWindows { get; set; } = "";
    public string PathLinux { get; set; } = "";
    public string PathMac { get; set; } = "";
    public bool Startup { get; set; } = false;
    public int StartupPriority { get; set; }
    public ResourceType Type { get; set; }
    public ContentType ContentType { get; set; }
    public string Extension { get; set; } = "";
    public string Args { get; set; } = "";
}