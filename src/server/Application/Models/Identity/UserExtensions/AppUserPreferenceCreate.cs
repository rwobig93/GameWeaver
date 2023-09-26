using Domain.Enums.Identity;
using Domain.Models.Identity;
using Newtonsoft.Json;

namespace Application.Models.Identity.UserExtensions;

public class AppUserPreferenceCreate
{
    public Guid OwnerId { get; set; }
    public AppThemeId ThemePreference { get; set; } = AppThemeId.Dark;
    public bool DrawerDefaultOpen { get; set; } = true;
    public string? CustomThemeOne { get; set; } = JsonConvert.SerializeObject(AppThemeCustom.GetExampleCustomOne());
    public string? CustomThemeTwo { get; set; } = JsonConvert.SerializeObject(AppThemeCustom.GetExampleCustomTwo());
    public string? CustomThemeThree { get; set; } = JsonConvert.SerializeObject(AppThemeCustom.GetExampleCustomThree());
}