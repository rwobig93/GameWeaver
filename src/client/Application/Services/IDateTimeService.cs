namespace Application.Services;

public interface IDateTimeService
{
    DateTime NowDatabaseTime { get; }
    DateTime NowFromTimeZone(string timeZoneId);
}