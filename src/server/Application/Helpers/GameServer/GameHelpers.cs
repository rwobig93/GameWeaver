namespace Application.Helpers.GameServer;

public static class GameHelpers
{
    public static string SanitizeFromSteam(this string name)
    {
        return name
            .Replace("dedicated", "", StringComparison.InvariantCulture)
            .Replace("server", "", StringComparison.InvariantCulture)
            .Replace("windows", "", StringComparison.InvariantCulture)
            .Replace("linux", "", StringComparison.InvariantCulture)
            .Replace("multiplayer", "", StringComparison.InvariantCulture)
            .Replace("test", "", StringComparison.InvariantCulture)
            .Replace("-", "")
            .Trim();
    }
}