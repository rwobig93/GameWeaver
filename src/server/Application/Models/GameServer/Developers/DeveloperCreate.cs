namespace Application.Models.GameServer.Developers;

public class DeveloperCreate
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
}