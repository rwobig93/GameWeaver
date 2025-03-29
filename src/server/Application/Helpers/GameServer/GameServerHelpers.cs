using Application.Constants.Identity;
using Application.Models.GameServer.GameServer;
using Application.Requests.GameServer.GameServer;
using Domain.DatabaseEntities.GameServer;
using Domain.DatabaseEntities.Identity;
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
}