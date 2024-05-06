using Domain.Enums.GameServer;

namespace Application.Models.GameServer.LocalResource;

public class LocalResourceUpdate
{
    public Guid Id { get; set; }
    public Guid? GameProfileId { get; set; }
    public Guid? GameServerId { get; set; }
    public string? Name { get; set; }
    public string? Path { get; set; }
    public bool? Startup { get; set; }
    public int? StartupPriority { get; set; }
    public ResourceType? Type { get; set; }
    public ContentType? ContentType { get; set; }
    public string? Extension { get; set; }
    public string? Args { get; set; }
}