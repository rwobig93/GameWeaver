using Domain.Enums.GameServer;

namespace Domain.DatabaseEntities.GameServer;

public class LocalResourceDb
{
    public Guid Id { get; set; }
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
    public bool LoadExisting { get; set; } = true;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}