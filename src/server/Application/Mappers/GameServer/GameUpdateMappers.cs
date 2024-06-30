using Application.Models.GameServer.GameUpdate;
using Domain.DatabaseEntities.GameServer;

namespace Application.Mappers.GameServer;

public static class GameUpdateMappers
{
    public static GameUpdateSlim ToSlim(this GameUpdateDb gameUpdate)
    {
        return new GameUpdateSlim
        {
            Id = gameUpdate.Id,
            GameId = gameUpdate.GameId,
            SupportsWindows = gameUpdate.SupportsWindows,
            SupportsLinux = gameUpdate.SupportsLinux,
            SupportsMac = gameUpdate.SupportsMac,
            BuildVersion = gameUpdate.BuildVersion,
            BuildVersionReleased = gameUpdate.BuildVersionReleased
        };
    }

    public static IEnumerable<GameUpdateSlim> ToSlims(this IEnumerable<GameUpdateDb> gameUpdates)
    {
        return gameUpdates.Select(ToSlim);
    }
}