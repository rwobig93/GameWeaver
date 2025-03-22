using Domain.Enums.GameServer;

namespace Application.Models.GameServer.LocalResource;

public class LocalResourceCreate
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid GameProfileId { get; set; }
    public string Name { get; set; } = "";
    public string PathWindows { get; set; } = "";
    public string PathLinux { get; set; } = "";
    public string PathMac { get; set; } = "";
    public bool Startup { get; set; } = false;
    public int StartupPriority { get; set; } = 100;
    public ResourceType Type { get; set; }
    public ContentType ContentType { get; set; }
    public string Args { get; set; } = "";
    public bool LoadExisting { get; set; } = true;  // Whether to merge existing configuration or overwrite
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}