namespace Application.Models.GameServer.Publishers;

public class PublisherSlim
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
}