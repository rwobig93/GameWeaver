using Application.Models.Identity.UserExtensions;
using Domain.DatabaseEntities.Identity;
using Domain.Models.Identity;
using Newtonsoft.Json;

namespace Application.Mappers.Identity;

public static class UserPreferenceMappers
{
    public static AppUserPreferenceFull ToFull(this AppUserPreferenceDb preferenceDb)
    {
        return new AppUserPreferenceFull
        {
            Id = preferenceDb.Id,
            OwnerId = preferenceDb.OwnerId,
            ThemePreference = preferenceDb.ThemePreference,
            DrawerDefaultOpen = preferenceDb.DrawerDefaultOpen,
            CustomThemeOne = AppThemeCustom.GetExampleCustomOne(),
            CustomThemeTwo = AppThemeCustom.GetExampleCustomTwo(),
            CustomThemeThree = AppThemeCustom.GetExampleCustomThree(),
            GamerMode = preferenceDb.GamerMode,
        };
    }

    public static AppUserPreferenceDb ToDb(this AppUserPreferenceFull preferenceFull)
    {
        return new AppUserPreferenceDb
        {
            Id = preferenceFull.Id,
            OwnerId = preferenceFull.OwnerId,
            ThemePreference = preferenceFull.ThemePreference,
            DrawerDefaultOpen = preferenceFull.DrawerDefaultOpen,
            CustomThemeOne = "",
            CustomThemeTwo = "",
            CustomThemeThree = "",
            GamerMode = preferenceFull.GamerMode,
        };
    }

    public static AppUserPreferenceCreate ToCreate(this AppUserPreferenceDb preferenceDb)
    {
        return new AppUserPreferenceCreate
        {
            OwnerId = preferenceDb.OwnerId,
            ThemePreference = preferenceDb.ThemePreference,
            DrawerDefaultOpen = preferenceDb.DrawerDefaultOpen,
            CustomThemeOne = preferenceDb.CustomThemeOne,
            CustomThemeTwo = preferenceDb.CustomThemeTwo,
            CustomThemeThree = preferenceDb.CustomThemeThree,
            GamerMode = preferenceDb.GamerMode,
        };
    }

    public static AppUserPreferenceCreate ToCreate(this AppUserPreferenceUpdate preferenceUpdate)
    {
        return new AppUserPreferenceCreate
        {
            ThemePreference = preferenceUpdate.ThemePreference,
            DrawerDefaultOpen = preferenceUpdate.DrawerDefaultOpen,
            CustomThemeOne = preferenceUpdate.CustomThemeOne,
            CustomThemeTwo = preferenceUpdate.CustomThemeTwo,
            CustomThemeThree = preferenceUpdate.CustomThemeThree,
            GamerMode = preferenceUpdate.GamerMode,
        };
    }

    public static AppUserPreferenceDb ToDb(this AppUserPreferenceCreate preferenceCreate)
    {
        return new AppUserPreferenceDb
        {
            OwnerId = preferenceCreate.OwnerId,
            ThemePreference = preferenceCreate.ThemePreference,
            DrawerDefaultOpen = preferenceCreate.DrawerDefaultOpen,
            CustomThemeOne = preferenceCreate.CustomThemeOne,
            CustomThemeTwo = preferenceCreate.CustomThemeTwo,
            CustomThemeThree = preferenceCreate.CustomThemeThree,
            GamerMode = preferenceCreate.GamerMode,
        };
    }

    public static AppUserPreferenceDb ToDb(this AppUserPreferenceUpdate preferenceUpdate)
    {
        return new AppUserPreferenceDb
        {
            Id = Guid.Empty,
            OwnerId = preferenceUpdate.OwnerId,
            ThemePreference = preferenceUpdate.ThemePreference,
            DrawerDefaultOpen = preferenceUpdate.DrawerDefaultOpen,
            CustomThemeOne = preferenceUpdate.CustomThemeOne,
            CustomThemeTwo = preferenceUpdate.CustomThemeTwo,
            CustomThemeThree = preferenceUpdate.CustomThemeThree,
            GamerMode = preferenceUpdate.GamerMode,
        };
    }

    public static AppUserPreferenceUpdate ToUpdate(this AppUserPreferenceDb preferenceDb)
    {
        return new AppUserPreferenceUpdate
        {
            Id = preferenceDb.Id,
            OwnerId = preferenceDb.OwnerId,
            ThemePreference = preferenceDb.ThemePreference,
            DrawerDefaultOpen = preferenceDb.DrawerDefaultOpen,
            CustomThemeOne = preferenceDb.CustomThemeOne,
            CustomThemeTwo = preferenceDb.CustomThemeTwo,
            CustomThemeThree = preferenceDb.CustomThemeThree,
            GamerMode = preferenceDb.GamerMode,
        };
    }

    public static AppUserPreferenceUpdate ToUpdate(this AppUserPreferenceFull preferenceDb)
    {
        return new AppUserPreferenceUpdate
        {
            Id = preferenceDb.Id,
            OwnerId = preferenceDb.OwnerId,
            ThemePreference = preferenceDb.ThemePreference,
            DrawerDefaultOpen = preferenceDb.DrawerDefaultOpen,
            CustomThemeOne = JsonConvert.SerializeObject(preferenceDb.CustomThemeOne),
            CustomThemeTwo = JsonConvert.SerializeObject(preferenceDb.CustomThemeTwo),
            CustomThemeThree = JsonConvert.SerializeObject(preferenceDb.CustomThemeThree),
            GamerMode = preferenceDb.GamerMode,
        };
    }
}