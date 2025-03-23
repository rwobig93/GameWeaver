using Domain.Enums.Identity;

namespace Application.Models.Identity.UserExtensions;

public class AppUserPreferenceUpdate
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public AppThemeId ThemePreference { get; set; } = AppThemeId.Dark;
    public bool DrawerDefaultOpen { get; set; }
    public string? CustomThemeOne { get; set; }
    public string? CustomThemeTwo { get; set; }
    public string? CustomThemeThree { get; set; }
    public bool GamerMode { get; set; }
}