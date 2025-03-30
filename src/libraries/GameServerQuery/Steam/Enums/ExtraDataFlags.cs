namespace GameServerQuery.Steam.Enums;

/// <summary>
/// See Extra Data Flag (EDF) in A2S_INFO Response Format: https://developer.valvesoftware.com/wiki/Server_queries#A2S_INFO
/// </summary>
public enum ExtraDataFlags : byte
{
    /// <summary>
    /// Data:Port | Type:short | Comment:The server's game port number.
    /// </summary>
    Port = 0x80,
    /// <summary>
    /// Data:SteamID | Type:long long | Comment:Server's SteamID.
    /// </summary>
    SteamId = 0x10,
    /// <summary>
    /// Data:Port | Type:short | Comment:Spectator port number for SourceTV.
    /// </summary>
    SourceTvPort = 0x40,
    /// <summary>
    /// Data:Name | Type:string | Comment:Name of the spectator server for SourceTV.
    /// </summary>
    SourceTvServerName = 0x40,
    /// <summary>
    /// Data:Keywords | Type:string | Comment:Tags that describe the game according to the server (for future use).
    /// </summary>
    Keywords = 0x20,
    /// <summary>
    /// Data:GameID | Type:long long | Comment:The server's 64-bit GameID. If this is present, a more accurate AppID is present in the low 24 bits.
    /// The earlier AppID could have been truncated as it was forced into 16-bit storage.
    /// </summary>
    GameId = 0x01
}