using Domain.Enums.GameServer;

namespace Application.Models.GameServer.LocalResource;

public class LocalResourceUpdate
{
    public Guid Id { get; set; }
    public Guid? GameProfileId { get; set; }
    public string? Name { get; set; }
    public string? PathWindows { get; set; }
    public string? PathLinux { get; set; }
    public string? PathMac { get; set; }
    public bool? Startup { get; set; }
    public int? StartupPriority { get; set; }
    public ResourceType? Type { get; set; }
    public ContentType? ContentType { get; set; }
    public string? Args { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}