using System.Diagnostics;
using GameServerQuery.Client;
using GameServerQuery.Parsers;
using GameServerQuery.Steam.Constants;
using GameServerQuery.Steam.Enums;
using GameServerQuery.Steam.Models;

namespace GameServerQuery.Steam;

public class SteamServerQuery : ServerClientBase
{
    /// <summary>
    /// Get general server information from the connected server
    /// </summary>
    /// <returns>Result w/ result state, message and response if successful</returns>
    public async Task<SteamServerQueryResult<SteamServerInfo>> GetInfo()
    {
        if (!IsConnected)
        {
            return new SteamServerQueryResult<SteamServerInfo> {Succeeded = false, Message = "Connection to the server is currently not active"};
        }

        var sendRequestTime = Stopwatch.GetTimestamp();
        var sendResult = await SendAsync(SteamServerMessages.InfoQuery);
        if (!sendResult.Succeeded)
        {
            return new SteamServerQueryResult<SteamServerInfo> {Succeeded = false, Message = sendResult.Message};
        }

        var receiveResult = await ReceiveAsync();
        if (!receiveResult.Succeeded)
        {
            return new SteamServerQueryResult<SteamServerInfo> {Succeeded = false, Message = receiveResult.Message};
        }

        var responseLatency = Stopwatch.GetElapsedTime(sendRequestTime);
        var serverInfo = new SteamServerInfo {Latency = responseLatency.Milliseconds};
        var parser = new PayloadParser(receiveResult.Response);

        try
        {
            // For response payload structure see: https://developer.valvesoftware.com/wiki/Server_queries#Response_Format
            parser.SkipBytes(SteamParsingConstants.HeaderPreambleBytesSize);
            var responseHeader = parser.ReadByte();
            if (responseHeader != (byte)SteamChallengeResponseHeader.A2SInfo)
            {
                return new SteamServerQueryResult<SteamServerInfo>
                {
                    Succeeded = false,
                    Message = $"Invalid header received from server for Server Info Query | Expected: {(byte)SteamChallengeResponseHeader.A2SInfo} | Received: {responseHeader}"
                };
            }
            serverInfo.Protocol = parser.ReadByte();
            serverInfo.Name = parser.ReadString();
            serverInfo.Map = parser.ReadString();
            serverInfo.Folder = parser.ReadString();
            serverInfo.Game = parser.ReadString();
            serverInfo.Id = parser.ReadUShort();
            serverInfo.Players = parser.ReadByte();
            serverInfo.MaxPlayers = parser.ReadByte();
            serverInfo.Bots = parser.ReadByte();
            serverInfo.ServerType = (char) parser.ReadByte() switch
            {
                'd' => SteamServerType.Dedicated,
                'l' => SteamServerType.NonDedicated,
                'p' => SteamServerType.SourceTvRelayProxy,
                _ => SteamServerType.Unknown
            };
            serverInfo.Environment = (char) parser.ReadByte() switch
            {
                'l' => SteamServerEnvironment.Linux,
                'w' => SteamServerEnvironment.Windows,
                'm' => SteamServerEnvironment.MacOs,
                'o' => SteamServerEnvironment.MacOs,
                _ => SteamServerEnvironment.Linux
            };
            serverInfo.HasPassword = parser.ReadByte() > 0;
            serverInfo.VacEnabled = parser.ReadByte() > 0;
            if (SteamParsingConstants.TheShipIds.Contains(serverInfo.Id))
            {
                serverInfo.TheShipMode = parser.ReadByte() switch
                {
                    0 => SteamTheShipMode.Hunt,
                    1 => SteamTheShipMode.Elimination,
                    2 => SteamTheShipMode.Duel,
                    3 => SteamTheShipMode.Deathmatch,
                    4 => SteamTheShipMode.VipTeam,
                    5 => SteamTheShipMode.TeamElimination,
                    _ => SteamTheShipMode.Unknown
                };
                serverInfo.TheShipWitnesses = parser.ReadByte();
                serverInfo.TheShipDuration = parser.ReadByte();
            }
            serverInfo.Version = parser.ReadString();
            if (!parser.HasUnparsedBytes)
            {
                return new SteamServerQueryResult<SteamServerInfo> {Succeeded = true, Response = serverInfo};
            }
        }
        catch (EndOfStreamException)
        {
            return new SteamServerQueryResult<SteamServerInfo> {Succeeded = false, Message = "Response payload from the server is malformed or not supported"};
        }

        try
        {
            serverInfo.ExtraDataFlag = parser.ReadByte();
            serverInfo.Port = parser.ReadUShort();
            serverInfo.SteamId = parser.ReadULong();
            serverInfo.SourceTvPort = parser.ReadUShort();
            serverInfo.SourceTvName = parser.ReadString();
            serverInfo.Keywords = parser.ReadString();
            serverInfo.GameId = parser.ReadULong();

            return new SteamServerQueryResult<SteamServerInfo> {Succeeded = true, Response = serverInfo};
        }
        catch (EndOfStreamException)
        {
            return new SteamServerQueryResult<SteamServerInfo>
            {
                Succeeded = true,
                Response = serverInfo,
                Message = "Retrieved most server info, extra data flags were present but couldn't be parsed, likely due to malformed or unsupported data"
            };
        }
    }

    /// <summary>
    /// Request a challenge value to initiate a player query to the connected server
    /// </summary>
    /// <returns>Result w/ result state, message and response if successful</returns>
    private async Task<SteamServerQueryResult<byte[]>> PlayerChallenge()
    {
        if (!IsConnected)
        {
            return new SteamServerQueryResult<byte[]> {Succeeded = false, Message = "Connection to the server is currently not active"};
        }

        var sendResult = await SendAsync(SteamServerMessages.PlayerQueryChallenge);
        if (!sendResult.Succeeded)
        {
            return new SteamServerQueryResult<byte[]> {Succeeded = false, Message = sendResult.Message};
        }

        var receiveResult = await ReceiveAsync();
        if (!receiveResult.Succeeded)
        {
            return new SteamServerQueryResult<byte[]> {Succeeded = false, Message = receiveResult.Message};
        }

        var parser = new PayloadParser(receiveResult.Response);
        try
        {
            // For challenge payload and response structure see: https://developer.valvesoftware.com/wiki/Server_queries#Request_Format_2
            parser.SkipBytes(SteamParsingConstants.HeaderPreambleBytesSize);
            var responseHeader = parser.ReadByte();
            if (responseHeader != (byte)SteamChallengeResponseHeader.A2SChallenge)
            {
                return new SteamServerQueryResult<byte[]>
                {
                    Succeeded = false,
                    Message = $"Invalid header received from server | Expected: {(byte)SteamChallengeResponseHeader.A2SChallenge} | Received: {responseHeader}"
                };
            }

            byte[] challengeResponse = [parser.ReadByte(), parser.ReadByte(), parser.ReadByte(), parser.ReadByte()];
            return new SteamServerQueryResult<byte[]> {Succeeded = true, Response = challengeResponse};
        }
        catch (EndOfStreamException)
        {
            return new SteamServerQueryResult<byte[]> {Succeeded = false, Message = "Response payload from the server is malformed or not supported"};
        }
    }

    /// <summary>
    /// Get players and their session details from the connected server
    /// </summary>
    /// <returns>Result w/ result state, message and response if successful</returns>
    public async Task<SteamServerQueryResult<SteamServerPlayersInfo>> GetPlayers()
    {
        if (!IsConnected)
        {
            return new SteamServerQueryResult<SteamServerPlayersInfo> {Succeeded = false, Message = "Connection to the server is currently not active"};
        }

        var challengeResult = await PlayerChallenge();
        if (!challengeResult.Succeeded || challengeResult.Response is null)
        {
            return new SteamServerQueryResult<SteamServerPlayersInfo> {Succeeded = false, Message = challengeResult.Message};
        }

        var queryAndChallenge = SteamServerMessages.PlayerQuery.Concat(challengeResult.Response).ToArray();
        var sendResult = await SendAsync(queryAndChallenge);
        if (!sendResult.Succeeded)
        {
            return new SteamServerQueryResult<SteamServerPlayersInfo> {Succeeded = false, Message = sendResult.Message};
        }

        var receiveResult = await ReceiveAsync();
        if (!receiveResult.Succeeded)
        {
            return new SteamServerQueryResult<SteamServerPlayersInfo> {Succeeded = false, Message = receiveResult.Message};
        }

        var playersInfo = new SteamServerPlayersInfo();
        var parser = new PayloadParser(receiveResult.Response);
        try
        {
            // For response payload structure see: https://developer.valvesoftware.com/wiki/Server_queries#Response_Format_2
            parser.SkipBytes(SteamParsingConstants.HeaderPreambleBytesSize);
            var responseHeader = parser.ReadByte();
            if (responseHeader != (byte)SteamChallengeResponseHeader.A2SPlayer)
            {
                return new SteamServerQueryResult<SteamServerPlayersInfo>
                {
                    Succeeded = false,
                    Message = $"Invalid header received from server | Expected: {(byte)SteamChallengeResponseHeader.A2SPlayer} | Received: {responseHeader}"
                };
            }

            var playerCount = (int)parser.ReadByte();
            playersInfo.PlayerCount = playerCount;
            for (var i = 0; i < playerCount; i++)
            {
                // A footnote mentioned is that players connecting can appear in the player count but not have information, we'll check for available bytes just in case
                if (!parser.HasUnparsedBytes)
                {
                    break;
                }

                var player = new SteamServerPlayer
                {
                    Index = parser.ReadByte(),
                    Name = parser.ReadString(),
                    Score = parser.ReadULong(),
                    Duration = TimeSpan.FromSeconds(parser.ReadFloat())
                };
                playersInfo.Players.Add(player);
            }

            if (!parser.HasUnparsedBytes)
            {
                return new SteamServerQueryResult<SteamServerPlayersInfo> {Succeeded = true, Response = playersInfo};
            }
        }
        catch (EndOfStreamException)
        {
            return new SteamServerQueryResult<SteamServerPlayersInfo> {Succeeded = false, Message = "Response payload from the server is malformed or not supported"};
        }

        try
        {
            playersInfo.TheShipDeaths = parser.ReadULong();
            playersInfo.TheShipMoney = parser.ReadULong();

            return new SteamServerQueryResult<SteamServerPlayersInfo> {Succeeded = true, Response = playersInfo};
        }
        catch (EndOfStreamException)
        {
            return new SteamServerQueryResult<SteamServerPlayersInfo>
            {
                Succeeded = true,
                Response = playersInfo,
                Message = "Retrieved player info but couldn't parse TheShip detail with the leftover response payload, likely due to malformed or unsupported data"
            };
        }
    }

    /// <summary>
    /// Request a challenge value to initiate a rules query to the connected server
    /// </summary>
    /// <returns>Result w/ result state, message and response if successful</returns>
    private async Task<SteamServerQueryResult<byte[]>> RulesChallenge()
    {
        if (!IsConnected)
        {
            return new SteamServerQueryResult<byte[]> {Succeeded = false, Message = "Connection to the server is currently not active"};
        }

        var sendResult = await SendAsync(SteamServerMessages.RuleQueryChallenge);
        if (!sendResult.Succeeded)
        {
            return new SteamServerQueryResult<byte[]> {Succeeded = false, Message = sendResult.Message};
        }

        var receiveResult = await ReceiveAsync();
        if (!receiveResult.Succeeded)
        {
            return new SteamServerQueryResult<byte[]> {Succeeded = false, Message = receiveResult.Message};
        }

        var parser = new PayloadParser(receiveResult.Response);
        try
        {
            // For challenge payload and response structure see: https://developer.valvesoftware.com/wiki/Server_queries#Request_Format_3
            parser.SkipBytes(SteamParsingConstants.HeaderPreambleBytesSize);
            var responseHeader = parser.ReadByte();
            if (responseHeader != (byte)SteamChallengeResponseHeader.A2SChallenge)
            {
                return new SteamServerQueryResult<byte[]>
                {
                    Succeeded = false,
                    Message = $"Invalid header received from server | Expected: {(byte)SteamChallengeResponseHeader.A2SChallenge} | Received: {responseHeader}"
                };
            }

            byte[] challengeResponse = [parser.ReadByte(), parser.ReadByte(), parser.ReadByte(), parser.ReadByte()];
            return new SteamServerQueryResult<byte[]> {Succeeded = true, Response = challengeResponse};
        }
        catch (EndOfStreamException)
        {
            return new SteamServerQueryResult<byte[]> {Succeeded = false, Message = "Response payload from the server is malformed or not supported"};
        }
    }

    /// <summary>
    /// Get server rules from the connected server
    /// </summary>
    /// <returns>Result w/ result state, message and response if successful</returns>
    public async Task<SteamServerQueryResult<SteamServerRulesInfo>> GetRules()
    {
        if (!IsConnected)
        {
            return new SteamServerQueryResult<SteamServerRulesInfo> {Succeeded = false, Message = "Connection to the server is currently not active"};
        }

        var challengeResult = await RulesChallenge();
        if (!challengeResult.Succeeded || challengeResult.Response is null)
        {
            return new SteamServerQueryResult<SteamServerRulesInfo> {Succeeded = false, Message = challengeResult.Message};
        }

        var queryAndChallenge = SteamServerMessages.RuleQuery.Concat(challengeResult.Response).ToArray();
        var sendResult = await SendAsync(queryAndChallenge);
        if (!sendResult.Succeeded)
        {
            return new SteamServerQueryResult<SteamServerRulesInfo> {Succeeded = false, Message = sendResult.Message};
        }

        var receiveResult = await ReceiveAsync();
        if (!receiveResult.Succeeded)
        {
            return new SteamServerQueryResult<SteamServerRulesInfo> {Succeeded = false, Message = receiveResult.Message};
        }

        var rulesInfo = new SteamServerRulesInfo();
        var parser = new PayloadParser(receiveResult.Response);
        try
        {
            // For response payload structure see: https://developer.valvesoftware.com/wiki/Server_queries#Response_Format_3
            parser.SkipBytes(SteamParsingConstants.HeaderPreambleBytesSize);
            var responseHeader = parser.ReadByte();
            if (responseHeader != (byte)SteamChallengeResponseHeader.A2SRules)
            {
                return new SteamServerQueryResult<SteamServerRulesInfo>
                {
                    Succeeded = false,
                    Message = $"Invalid header received from server | Expected: {(byte)SteamChallengeResponseHeader.A2SRules} | Received: {responseHeader}"
                };
            }

            rulesInfo.RuleCount = parser.ReadUShort();
            for (var i = 0; i < rulesInfo.RuleCount; i++)
            {
                var rule = new SteamServerRule
                {
                    Name = parser.ReadString(),
                    Value = parser.ReadString()
                };
                rulesInfo.Rules.Add(rule);
            }

            return new SteamServerQueryResult<SteamServerRulesInfo> {Succeeded = true, Response = rulesInfo};
        }
        catch (EndOfStreamException)
        {
            return new SteamServerQueryResult<SteamServerRulesInfo> {Succeeded = false, Message = "Response payload from the server is malformed or not supported"};
        }
    }
}