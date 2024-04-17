using Domain.Models.GameServer;

namespace Application.Helpers;

public static class GameServerHelpers
{
    public static string GetInstallDirectory(this GameServerLocal gameServerLocal)
    {
        return Path.Join(OsHelpers.GetDefaultGameServerPath(), gameServerLocal.Id.ToString());
    }
}