using Domain.Enums.GameServer;

namespace Application.Models.GameServer.LocalResource;

public class LocalResourceCreate
{
    public Guid GameProfileId { get; set; }
    // TODO: Remove GameServerId, resource is bound only to a profile, up to 3 profiles (default, parent, server) are used together 
    public Guid GameServerId { get; set; }
    public string Name { get; set; } = "";
    public string PathWindows { get; set; } = "";
    public string PathLinux { get; set; } = "";
    public string PathMac { get; set; } = "";
    public bool Startup { get; set; } = false;
    public int StartupPriority { get; set; } = 100;
    public ResourceType Type { get; set; }
    public ContentType ContentType { get; set; }
    // TODO: Remove Extension, since this changes per OS we want to use Path* instead
    public string Extension { get; set; } = "";
    public string Args { get; set; } = "";
}