namespace Application.Models.GameServer.Developers;

public class DeveloperSlim
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
}