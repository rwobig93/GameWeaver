using Domain.Enums.Identity;
using Domain.Models.Identity;
using Newtonsoft.Json;

namespace Domain.DatabaseEntities.Identity;

public class AppUserPreferenceDb
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public AppThemeId ThemePreference { get; set; } = AppThemeId.Dark;
    public bool DrawerDefaultOpen { get; set; }
    public string? CustomThemeOne { get; set; } = JsonConvert.SerializeObject(new AppThemeCustom());
    public string? CustomThemeTwo { get; set; } = JsonConvert.SerializeObject(new AppThemeCustom());
    public string? CustomThemeThree { get; set; } = JsonConvert.SerializeObject(new AppThemeCustom());
    public bool GamerMode { get; set; }
    public string? Toggled { get; set; }
    public string? FavoriteGameServers { get; set; }
}