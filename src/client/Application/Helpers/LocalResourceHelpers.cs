using Domain.Models.GameServer;

namespace Application.Helpers;

public static class LocalResourceHelpers
{
    public static string GetFullPath(this LocalResource localResource)
    {
        var gameServerPath = Path.Join(OsHelpers.GetDefaultGameServerPath(), localResource.GameserverId.ToString());
        return Path.Join(gameServerPath, localResource.Path);
    }

    public static string UpdateWithServerValues(this GameServerLocal gameServer, string value)
    {
        return value
            .Replace("%%%SERVER_NAME%%%", gameServer.ServerName)
            .Replace("%%%PASSWORD%%%", gameServer.Password)
            .Replace("%%%QUERY_PORT%%%", gameServer.PortQuery.ToString())
            .Replace("%%%GAME_PORT%%%", gameServer.PortGame.ToString())
            .Replace("%%%GAME_PORT_PEER%%%", (gameServer.PortPeer).ToString())
            .Replace("%%%PASSWORD_ADMIN%%%", gameServer.PasswordAdmin)
            .Replace("%%%PASSWORD_RCON%%%", gameServer.PasswordRcon);
    }
}