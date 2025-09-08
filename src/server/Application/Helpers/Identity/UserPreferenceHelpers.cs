using Application.Models.Identity.UserExtensions;
using Domain.DatabaseEntities.Identity;
using Domain.Models.Identity;

namespace Application.Helpers.Identity;

public static class UserPreferenceHelpers
{
    public static string GetDbToggledValue(this AppUserPreferenceFull preference) => string.Join('|', preference.Toggled);

    public static string GetDbToggledValue(this AppUserPreferenceCreate preference) => string.Join('|', preference.Toggled);

    public static List<string> GetDbToggledValue(this AppUserPreferenceUpdate preference) =>
        string.IsNullOrWhiteSpace(preference.Toggled) ? [] : preference.Toggled.Split('|').ToList();

    public static List<string> GetToggledFromDb(this AppUserPreferenceDb preference) => string.IsNullOrWhiteSpace(preference.Toggled) ? [] : preference.Toggled.Split('|').ToList();

    public static string ToUserPreferenceToggledDb(this IEnumerable<string> toggled) => string.Join('|', toggled);

    public static string ToUserPreferenceFavoriteDb(this IEnumerable<string> favorites) => string.Join('|', favorites);

    public static bool HasToggled(this AppUserPreferenceFull preference, string value) => preference.Toggled.Contains(value);

    public static string GetDbFavoriteGameServerValue(this AppUserPreferenceFull preference) => string.Join('|', preference.FavoriteGameServers);

    public static string GetDbFavoriteGameServerValue(this AppUserPreferenceCreate preference) => string.Join('|', preference.FavoriteGameServers);

    public static List<string> GetDbFavoriteGameServerValue(this AppUserPreferenceUpdate preference) =>
        string.IsNullOrWhiteSpace(preference.FavoriteGameServers) ? [] : preference.FavoriteGameServers.Split('|').ToList();

    public static List<string> GetFavoriteGameServerFromDb(this AppUserPreferenceDb preference) =>
        string.IsNullOrWhiteSpace(preference.FavoriteGameServers) ? [] : preference.FavoriteGameServers.Split('|').ToList();

    public static bool HasFavoriteGameServer(this AppUserPreferenceFull preference, string id) => preference.FavoriteGameServers.Contains(id);

    public static bool HasFavoriteGameServer(this AppUserPreferenceFull preference, Guid id) => preference.HasFavoriteGameServer(id.ToString());

    public static IEnumerable<Guid> GetFavoriteGameServerIds(this AppUserPreferenceFull preference) => preference.FavoriteGameServers.Select(Guid.Parse);
}