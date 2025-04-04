using Application.Constants.Identity;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.Network;
using Application.Requests.GameServer.GameServer;
using Domain.DatabaseEntities.GameServer;
using Domain.DatabaseEntities.Identity;
using Domain.Enums.GameServer;
using Domain.Enums.Identity;

namespace Application.Helpers.GameServer;

public static class GameServerHelpers
{
    public static List<int> GetUsedPorts(this GameServerDb gameServer)
    {
        return [gameServer.PortGame, gameServer.PortPeer, gameServer.PortQuery, gameServer.PortRcon];
    }

    public static List<int> GetUsedPorts(this IEnumerable<GameServerDb> gameServers)
    {
        return gameServers.SelectMany(x => x.GetUsedPorts()).ToList();
    }

    public static List<int> GetUsedPorts(this GameServerSlim gameServer)
    {
        return [gameServer.PortGame, gameServer.PortPeer, gameServer.PortQuery, gameServer.PortRcon];
    }

    public static List<int> GetUsedPorts(this IEnumerable<GameServerSlim> gameServers)
    {
        return gameServers.SelectMany(x => x.GetUsedPorts()).ToList();
    }

    public static List<int> GetUsedPorts(this GameServerCreate gameServer)
    {
        return [gameServer.PortGame, gameServer.PortPeer, gameServer.PortQuery, gameServer.PortRcon];
    }

    public static List<int> GetUsedPorts(this IEnumerable<GameServerCreate> gameServers)
    {
        return gameServers.SelectMany(x => x.GetUsedPorts()).ToList();
    }

    public static List<int> GetUsedPorts(this GameServerCreateRequest gameServer)
    {
        return [gameServer.PortGame, gameServer.PortPeer, gameServer.PortQuery, gameServer.PortRcon];
    }

    public static List<int> GetUsedPorts(this IEnumerable<GameServerCreateRequest> gameServers)
    {
        return gameServers.SelectMany(x => x.GetUsedPorts()).ToList();
    }

    public static bool PermissionsHaveAccess(this GameServerDb gameServer, IEnumerable<AppPermissionDb> permissions)
    {
        return permissions.Select(x =>
            x.ClaimValue == PermissionConstants.GameServer.Gameserver.Dynamic(gameServer.Id, DynamicPermissionLevel.Admin) ||
            x.ClaimValue == PermissionConstants.GameServer.Gameserver.Dynamic(gameServer.Id, DynamicPermissionLevel.View)).Any();
    }

    public static GameServerConnectivityCheck GetConnectivityCheck(this GameServerDb gameServer, bool isSteam, NetworkProtocol protocol = NetworkProtocol.Tcp,
        int timeoutMs = 150, bool usePublicIp = true)
    {
        if (isSteam)
        {
            return new GameServerConnectivityCheck
            {
                HostIp = usePublicIp ? gameServer.PublicIp : gameServer.PrivateIp,
                PortGame = gameServer.PortGame,
                PortQuery = gameServer.PortQuery,
                Protocol = NetworkProtocol.Udp,
                TimeoutMilliseconds = timeoutMs,
                Source = GameSource.Steam
            };
        }

        return new GameServerConnectivityCheck
        {
            HostIp = usePublicIp ? gameServer.PublicIp : gameServer.PrivateIp,
            PortGame = gameServer.PortGame,
            PortQuery = gameServer.PortQuery,
            Protocol = protocol,
            TimeoutMilliseconds = timeoutMs,
            Source = GameSource.Manual
        };
    }
}