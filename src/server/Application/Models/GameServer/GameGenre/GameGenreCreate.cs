namespace Application.Models.GameServer.GameGenre;

public class GameGenreCreate
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}