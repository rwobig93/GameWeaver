namespace Application.Models.GameServer.Developers;

public class DeveloperCreate
{
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
}