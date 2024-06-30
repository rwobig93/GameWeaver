namespace Application.Models.Events;

public class GameVersionUpdatedEvent
{
    public Guid GameId { get; set; }
    public int AppId { get; set; }
    public string AppName { get; set; } = null!;
    public string VersionBuild { get; set; } = null!;
}