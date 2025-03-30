namespace GameServerQuery.Steam.Constants;

public static class SteamParsingConstants
{
    public const int ChallengeHeaderPreambleBytesSize = 4;

    /// <summary>
    /// ID's specific to parsing 'The Ship' fields, See: https://developer.valvesoftware.com/wiki/The_Ship
    /// </summary>
    public static readonly int[] TheShipIds = [2400, 2401, 2402, 2412, 2430, 2406, 2405];
}