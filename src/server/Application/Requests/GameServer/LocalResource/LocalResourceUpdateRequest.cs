using Domain.Enums.GameServer;

namespace Application.Requests.GameServer.LocalResource;

public class LocalResourceUpdateRequest
{
    public Guid GameProfileId { get; set; }
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? PathWindows { get; set; }
    public string? PathLinux { get; set; }
    public string? PathMac { get; set; }
    public bool? Startup { get; set; }
    public int? StartupPriority { get; set; }
    public ResourceType? Type { get; set; }
    public ContentType? ContentType { get; set; }
    public string? Args { get; set; }
    public bool? LoadExisting { get; set; }
}