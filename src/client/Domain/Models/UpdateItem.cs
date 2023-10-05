using Domain.Enums;

namespace Domain.Models;

public class UpdateItem
{
    public string Name { get; set; } = "";
    public int SteamId { get; set; }
    public string UpdateUri { get; set; } = "";
    public string CurrentVersion { get; set; } = "";
    public string NewVersion { get; set; } = "";
    public GameSource Source { get; set; }
}