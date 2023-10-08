namespace Application.Models.GameServer.GameGenre;

public class GameGenreSlim
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}