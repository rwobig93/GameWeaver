using Domain.DatabaseEntities.GameServer;

namespace Application.Services.Lifecycle;

public interface IJobManager
{
    Task UserHousekeeping();
    Task DailyCleanup();
    Task GameVersionCheck();
    Task DailySteamSync();
    Task HostStatusCheck();
    Task GameServerStatusCheck();
    Task UpdateGameServerStatus(GameServerDb gameserver);
}