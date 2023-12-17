using Application.Services.System;

namespace Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime NowDatabaseTime => DateTime.UtcNow;

    public DateTime NowFromTimeZone(string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
    }
}