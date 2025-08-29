using Application.Models.GameServer.ConfigurationItem;
using Domain.Enums.GameServer;

namespace Application.Models.GameServer.LocalResource;

public class LocalResourceExport
{
    public string Name { get; set; } = "";
    public string PathWindows { get; set; } = "";
    public string PathLinux { get; set; } = "";
    public string PathMac { get; set; } = "";
    public bool Startup { get; set; } = false;
    public int StartupPriority { get; set; }
    public ResourceType Type { get; set; }
    public ContentType ContentType { get; set; }
    public string Args { get; set; } = "";
    public bool LoadExisting { get; set; }
    public List<ConfigurationItemExport> Configuration { get; set; } = [];
}