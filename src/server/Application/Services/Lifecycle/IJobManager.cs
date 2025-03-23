namespace Application.Services.Lifecycle;

public interface IJobManager
{
    Task UserHousekeeping();
    Task DailyCleanup();
    Task GameVersionCheck();
    Task DailySteamSync();
    Task HostStatusCheck();
}