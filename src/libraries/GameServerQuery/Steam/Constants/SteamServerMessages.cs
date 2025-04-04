namespace GameServerQuery.Steam.Constants;

public static class SteamServerMessages
{
    // For supported steam query headers and protocol definitions see: https://developer.valvesoftware.com/wiki/Server_queries

    /// <summary>
    /// A2S_INFO: Retrieves information about the server including, but not limited to: its name, the map currently being played, and the number of players.
    /// </summary>
    public static readonly byte[] InfoQuery =
    [
        0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65,
        0x72, 0x79, 0x00
    ];

    /// <summary>
    /// A2S_PLAYER: This query retrieves information about the players currently on the server. It needs an initial step to acquire a challenge number.
    /// </summary>
    public static readonly byte[] PlayerQuery = [0xFF, 0xFF, 0xFF, 0xFF, 0x55];
    /// <summary>
    /// Challenge request for a A2S_PLAYER player query
    /// </summary>
    public static readonly byte[] PlayerQueryChallenge = [0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF];

    /// <summary>
    /// A2S_RULES: Returns the server rules, or configuration variables in name/value pairs. This query requires an initial challenge step.
    /// </summary>
    public static readonly byte[] RuleQuery = [0xFF, 0xFF, 0xFF, 0xFF, 0x56];
    /// <summary>
    /// Challenge request for a A2S_RULES rule query
    /// </summary>
    public static readonly byte[] RuleQueryChallenge = [0xFF, 0xFF, 0xFF, 0xFF, 0x56, 0xFF, 0xFF, 0xFF, 0xFF];

    /// <summary>
    /// A2S_PLAYER and A2S_RULES queries both require a challenge number. Formerly, this number could be obtained via an A2S_SERVERQUERY_GETCHALLENGE request.
    /// In newer games it no longer works. Instead, issue A2S_PLAYER or A2S_RULES queries with an initial challenge of -1 (0xFFFFFFFF) to receive a usable challenge number.
    /// </summary>
    /// <remarks>On some engines (confirmed AppIDs: 17510, 17530, 17740, 17550, 17700) it can be used.</remarks>
    public static readonly byte[] A2SServerQueryGetChallenge = [0xFF, 0xFF, 0xFF, 0xFF, 0x57];
}