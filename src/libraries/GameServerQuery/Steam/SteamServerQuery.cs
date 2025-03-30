using System.Diagnostics;
using GameServerQuery.Client;
using GameServerQuery.Parsers;
using GameServerQuery.Steam.Constants;
using GameServerQuery.Steam.Enums;
using GameServerQuery.Steam.Models;

namespace GameServerQuery.Steam;

public class SteamServerQuery : ServerClientBase
{
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
        var serverInfo = new SteamServerInfo {Latency = responseLatency.Seconds};
        var responseParser = new PayloadParser(receiveResult.Response);

        try
        {
            serverInfo.Protocol = responseParser.ReadByte();
            serverInfo.Name = responseParser.ReadString();
            serverInfo.Map = responseParser.ReadString();
            serverInfo.Folder = responseParser.ReadString();
            serverInfo.Game = responseParser.ReadString();
            serverInfo.Id = responseParser.ReadUShort();
            serverInfo.Players = responseParser.ReadByte();
            serverInfo.MaxPlayers = responseParser.ReadByte();
            serverInfo.Bots = responseParser.ReadByte();
            serverInfo.Type = (char) responseParser.ReadByte() switch
            {
                'd' => SteamServerType.Dedicated,
                'l' => SteamServerType.NonDedicated,
                'p' => SteamServerType.SourceTvRelayProxy,
                _ => SteamServerType.Unknown
            };
            serverInfo.OperatingSystem = (char) responseParser.ReadByte() switch
            {
                'l' => SteamServerEnvironment.Linux,
                'w' => SteamServerEnvironment.Windows,
                'm' => SteamServerEnvironment.MacOs,
                'o' => SteamServerEnvironment.MacOs,
                _ => SteamServerEnvironment.Linux
            };
            serverInfo.IsPrivate = responseParser.ReadByte() > 0;
            serverInfo.VacEnabled = responseParser.ReadByte() > 0;
            if (SteamParsingConstants.TheShipIds.Contains(serverInfo.Id))
            {
                serverInfo.TheShipMode = responseParser.ReadByte() switch
                {
                    0 => SteamTheShipMode.Hunt,
                    1 => SteamTheShipMode.Elimination,
                    2 => SteamTheShipMode.Duel,
                    3 => SteamTheShipMode.Deathmatch,
                    4 => SteamTheShipMode.VipTeam,
                    5 => SteamTheShipMode.TeamElimination,
                    _ => SteamTheShipMode.Unknown
                };
                serverInfo.TheShipWitnesses = responseParser.ReadByte();
                serverInfo.TheShipDuration = responseParser.ReadByte();
            }
            serverInfo.Version = responseParser.ReadString();
            if (responseParser.PayloadUnparsed <= 0)
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
            serverInfo.ExtraDataFlag = responseParser.ReadByte();
            serverInfo.Port = responseParser.ReadUShort();
            serverInfo.SteamId = responseParser.ReadULong();
            serverInfo.SourceTvPort = responseParser.ReadUShort();
            serverInfo.SourceTvName = responseParser.ReadString();
            serverInfo.Keywords = responseParser.ReadString();
            serverInfo.GameId = responseParser.ReadULong();
        
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
}