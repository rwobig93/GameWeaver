using Application.Models.GameServer.LocalResource;

namespace Application.Models.GameServer.GameProfile;

public class GameProfileExport
{
    public string Name { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public bool AllowAutoDelete { get; set; } = true;
    public List<LocalResourceExport> Resources { get; set; } = [];
}