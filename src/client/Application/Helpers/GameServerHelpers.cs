using Domain.Models.GameServer;
using Domain.Models.Network;

namespace Application.Helpers;

public static class GameServerHelpers
{
    public static string GetInstallDirectory(this GameServerLocal gameServerLocal)
    {
        return Path.Join(OsHelpers.GetDefaultGameServerPath(), gameServerLocal.Id.ToString());
    }

    public static List<NetworkListeningSocket> GetListeningSockets(this GameServerLocal gameServerLocal)
    {
        return OsHelpers.GetListeningSockets().Where(x =>
            x.Port == gameServerLocal.PortGame ||
            x.Port == gameServerLocal.PortGame + 1 ||
            x.Port == gameServerLocal.PortQuery)
            .ToList();
    }
}