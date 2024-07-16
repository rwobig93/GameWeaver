using System.Text.RegularExpressions;

namespace Application.Helpers.GameServer;

public static partial class GameHelpers
{
    public static string SanitizeFromSteam(this string name)
    {
        return SteamNameSanitizeRegex().Replace(name, string.Empty)
            .Replace("dedicated", string.Empty, StringComparison.InvariantCultureIgnoreCase)
            .Replace("server", string.Empty, StringComparison.InvariantCultureIgnoreCase)
            .Replace("servers", string.Empty, StringComparison.InvariantCultureIgnoreCase)
            .Replace("windows", string.Empty, StringComparison.InvariantCultureIgnoreCase)
            .Replace("linux", string.Empty, StringComparison.InvariantCultureIgnoreCase)
            .Replace("multiplayer", string.Empty, StringComparison.InvariantCultureIgnoreCase)
            .Replace("test", string.Empty, StringComparison.InvariantCultureIgnoreCase)
            .Replace(" beta", string.Empty, StringComparison.InvariantCultureIgnoreCase)
            .Replace("developer build", string.Empty, StringComparison.InvariantCultureIgnoreCase)
            .Replace(@"[^a-zA-Z0-9]", string.Empty)
            .Trim();
    }

    [GeneratedRegex(@"[^a-zA-Z0-9:]\s")]
    private static partial Regex SteamNameSanitizeRegex();
}