using Domain.Enums.Identity;

namespace Domain.Models.Identity;

public class AppUserPreferenceFull
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public AppThemeId ThemePreference { get; set; } = AppThemeId.Dark;
    public bool DrawerDefaultOpen { get; set; }
    public AppThemeCustom CustomThemeOne { get; set; } = AppThemeCustom.GetExampleCustomOne();
    public AppThemeCustom CustomThemeTwo { get; set; } = AppThemeCustom.GetExampleCustomTwo();
    public AppThemeCustom CustomThemeThree { get; set; } = AppThemeCustom.GetExampleCustomThree();
    public bool GamerMode { get; set; }
    public List<string> Toggled { get; set; } = [];
    public List<string> FavoriteGameServers { get; set; } = [];
}