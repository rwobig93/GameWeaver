namespace Application.Services.System;

public interface IDateTimeService
{
    DateTime NowDatabaseTime { get; }
    DateTime NowFromTimeZone(string timeZoneId);
}