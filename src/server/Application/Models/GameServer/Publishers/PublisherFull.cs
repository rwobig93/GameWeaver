using Application.Models.GameServer.Game;

namespace Application.Models.GameServer.Publishers;

public class PublisherFull
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
    public List<GameSlim> Games { get; set; } = new();
}