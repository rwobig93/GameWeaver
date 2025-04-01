namespace Application.Models.GameServer.Publishers;

public class PublisherCreate
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
}