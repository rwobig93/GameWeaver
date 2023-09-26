using Domain.Enums.Identity;

namespace GameWeaver.Settings;

public class AppTheme
{
    public AppThemeId Id { get; set; }
    public string FriendlyName { get; set; } = "A Theme";
    public string Description { get; set; } = "There may be colors in your future";
    public MudTheme Theme { get; set; } = null!;
    public string Icon { get; set; } = Icons.Material.Filled.ColorLens;
}