using Application.Models.GameServer.GameServer;
using Application.Requests.GameServer.GameServer;
using Domain.DatabaseEntities.GameServer;

namespace Application.Helpers.GameServer;

public static class GameServerHelpers
{
    public static List<int> GetUsedPorts(this GameServerDb gameServer)
    {
        // TODO: Add PortPeer for more granular port control, most games use the next port after the game port but few are configurable
        return [gameServer.PortGame, gameServer.PortGame + 1, gameServer.PortQuery, gameServer.PortRcon];
    }

    public static List<int> GetUsedPorts(this IEnumerable<GameServerDb> gameServers)
    {
        return gameServers.SelectMany(x => x.GetUsedPorts()).ToList();
    }
    
    public static List<int> GetUsedPorts(this GameServerCreate gameServer)
    {
        return [gameServer.PortGame, gameServer.PortGame + 1, gameServer.PortQuery, gameServer.PortRcon];
    }

    public static List<int> GetUsedPorts(this IEnumerable<GameServerCreate> gameServers)
    {
        return gameServers.SelectMany(x => x.GetUsedPorts()).ToList();
    }
    
    public static List<int> GetUsedPorts(this GameServerCreateRequest gameServer)
    {
        return [gameServer.PortGame, gameServer.PortGame + 1, gameServer.PortQuery, gameServer.PortRcon];
    }

    public static List<int> GetUsedPorts(this IEnumerable<GameServerCreateRequest> gameServers)
    {
        return gameServers.SelectMany(x => x.GetUsedPorts()).ToList();
    }
}