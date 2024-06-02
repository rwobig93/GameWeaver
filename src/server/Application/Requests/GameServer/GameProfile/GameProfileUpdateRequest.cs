namespace Application.Requests.GameServer.GameProfile;

public class GameProfileUpdateRequest
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid? OwnerId { get; set; }
}