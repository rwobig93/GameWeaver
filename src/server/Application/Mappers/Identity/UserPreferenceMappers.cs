using Application.Helpers.Identity;
using Application.Models.Identity.UserExtensions;
using Domain.DatabaseEntities.Identity;
using Domain.Models.Identity;
using Newtonsoft.Json;

namespace Application.Mappers.Identity;

public static class UserPreferenceMappers
{
    public static AppUserPreferenceFull ToFull(this AppUserPreferenceDb preference)
    {
        return new AppUserPreferenceFull
        {
            Id = preference.Id,
            OwnerId = preference.OwnerId,
            ThemePreference = preference.ThemePreference,
            DrawerDefaultOpen = preference.DrawerDefaultOpen,
            CustomThemeOne = AppThemeCustom.GetExampleCustomOne(),
            CustomThemeTwo = AppThemeCustom.GetExampleCustomTwo(),
            CustomThemeThree = AppThemeCustom.GetExampleCustomThree(),
            GamerMode = preference.GamerMode,
            Toggled = preference.GetToggledFromDb()
        };
    }

    public static AppUserPreferenceDb ToDb(this AppUserPreferenceFull preference)
    {
        return new AppUserPreferenceDb
        {
            Id = preference.Id,
            OwnerId = preference.OwnerId,
            ThemePreference = preference.ThemePreference,
            DrawerDefaultOpen = preference.DrawerDefaultOpen,
            CustomThemeOne = "",
            CustomThemeTwo = "",
            CustomThemeThree = "",
            GamerMode = preference.GamerMode,
            Toggled = preference.GetDbToggledValue()
        };
    }

    public static AppUserPreferenceCreate ToCreate(this AppUserPreferenceDb preference)
    {
        return new AppUserPreferenceCreate
        {
            Id = Guid.CreateVersion7(),
            OwnerId = preference.OwnerId,
            ThemePreference = preference.ThemePreference,
            DrawerDefaultOpen = preference.DrawerDefaultOpen,
            CustomThemeOne = preference.CustomThemeOne,
            CustomThemeTwo = preference.CustomThemeTwo,
            CustomThemeThree = preference.CustomThemeThree,
            GamerMode = preference.GamerMode,
            Toggled = preference.GetToggledFromDb()
        };
    }

    public static AppUserPreferenceCreate ToCreate(this AppUserPreferenceUpdate preference)
    {
        return new AppUserPreferenceCreate
        {
            Id = Guid.CreateVersion7(),
            OwnerId = preference.OwnerId,
            ThemePreference = preference.ThemePreference,
            DrawerDefaultOpen = preference.DrawerDefaultOpen,
            CustomThemeOne = preference.CustomThemeOne,
            CustomThemeTwo = preference.CustomThemeTwo,
            CustomThemeThree = preference.CustomThemeThree,
            GamerMode = preference.GamerMode,
            Toggled = preference.GetDbToggledValue()
        };
    }

    public static AppUserPreferenceDb ToDb(this AppUserPreferenceCreate preference)
    {
        return new AppUserPreferenceDb
        {
            Id = Guid.CreateVersion7(),
            OwnerId = preference.OwnerId,
            ThemePreference = preference.ThemePreference,
            DrawerDefaultOpen = preference.DrawerDefaultOpen,
            CustomThemeOne = preference.CustomThemeOne,
            CustomThemeTwo = preference.CustomThemeTwo,
            CustomThemeThree = preference.CustomThemeThree,
            GamerMode = preference.GamerMode,
            Toggled = preference.GetDbToggledValue()
        };
    }

    public static AppUserPreferenceDb ToDb(this AppUserPreferenceUpdate preference)
    {
        return new AppUserPreferenceDb
        {
            Id = Guid.Empty,
            OwnerId = preference.OwnerId,
            ThemePreference = preference.ThemePreference,
            DrawerDefaultOpen = preference.DrawerDefaultOpen,
            CustomThemeOne = preference.CustomThemeOne,
            CustomThemeTwo = preference.CustomThemeTwo,
            CustomThemeThree = preference.CustomThemeThree,
            GamerMode = preference.GamerMode,
            Toggled = null
        };
    }

    public static AppUserPreferenceUpdate ToUpdate(this AppUserPreferenceDb preference)
    {
        return new AppUserPreferenceUpdate
        {
            Id = preference.Id,
            OwnerId = preference.OwnerId,
            ThemePreference = preference.ThemePreference,
            DrawerDefaultOpen = preference.DrawerDefaultOpen,
            CustomThemeOne = preference.CustomThemeOne,
            CustomThemeTwo = preference.CustomThemeTwo,
            CustomThemeThree = preference.CustomThemeThree,
            GamerMode = preference.GamerMode,
            Toggled = preference.Toggled
        };
    }

    public static AppUserPreferenceUpdate ToUpdate(this AppUserPreferenceFull preference)
    {
        return new AppUserPreferenceUpdate
        {
            Id = preference.Id,
            OwnerId = preference.OwnerId,
            ThemePreference = preference.ThemePreference,
            DrawerDefaultOpen = preference.DrawerDefaultOpen,
            CustomThemeOne = JsonConvert.SerializeObject(preference.CustomThemeOne),
            CustomThemeTwo = JsonConvert.SerializeObject(preference.CustomThemeTwo),
            CustomThemeThree = JsonConvert.SerializeObject(preference.CustomThemeThree),
            GamerMode = preference.GamerMode,
            Toggled = preference.GetDbToggledValue()
        };
    }

    public static AppUserPreferenceUpdate ToUpdateToggled(this AppUserPreferenceFull preference)
    {
        return new AppUserPreferenceUpdate
        {
            Id = preference.Id,
            OwnerId = preference.OwnerId,
            ThemePreference = preference.ThemePreference,
            DrawerDefaultOpen = preference.DrawerDefaultOpen,
            CustomThemeOne = null,
            CustomThemeTwo = null,
            CustomThemeThree = null,
            GamerMode = preference.GamerMode,
            Toggled = preference.GetDbToggledValue()
        };
    }
}