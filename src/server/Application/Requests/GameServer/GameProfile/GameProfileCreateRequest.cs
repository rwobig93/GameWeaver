namespace Application.Requests.GameServer.GameProfile;

public class GameProfileCreateRequest
{
    public string Name { get; set; } = null!;
    public Guid OwnerId { get; set; }
    public Guid GameId { get; set; }
}