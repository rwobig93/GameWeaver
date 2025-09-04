using Application.Models.Identity.UserExtensions;
using Domain.DatabaseEntities.Identity;
using Domain.Models.Identity;

namespace Application.Helpers.Identity;

public static class UserPreferenceHelpers
{
    public static string GetDbToggledValue(this AppUserPreferenceFull preference) => string.Join('|', preference.Toggled);

    public static string GetDbToggledValue(this AppUserPreferenceCreate preference) => string.Join('|', preference.Toggled);

    public static List<string> GetDbToggledValue(this AppUserPreferenceUpdate preference) => preference.Toggled?.Split('|').ToList() ?? [];

    public static List<string> GetToggledFromDb(this AppUserPreferenceDb preference) => preference.Toggled?.Split('|').ToList() ?? [];

    public static bool HasToggled(this AppUserPreferenceFull preference, string value) => preference.Toggled.Contains(value);
}