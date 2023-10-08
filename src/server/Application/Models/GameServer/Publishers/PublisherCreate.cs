namespace Application.Models.GameServer.Publishers;

public class PublisherCreate
{
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
}