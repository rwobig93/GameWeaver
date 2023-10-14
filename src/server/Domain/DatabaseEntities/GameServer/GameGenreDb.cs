namespace Domain.DatabaseEntities.GameServer;

public class GameGenreDb
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}