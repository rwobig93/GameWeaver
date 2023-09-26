using Application.Services.System;
using Domain.Enums.Identity;

namespace Application.Helpers.Auth;

public static class UserApiHelpers
{
    public static DateTime GetUserApiTokenExpirationTime(IDateTimeService dateTime, UserApiTokenTimeframe timeframe)
    {
        return timeframe switch
        {
            UserApiTokenTimeframe.OneDay => dateTime.NowDatabaseTime.AddDays(1),
            UserApiTokenTimeframe.OneWeek => dateTime.NowDatabaseTime.AddDays(7),
            UserApiTokenTimeframe.TwoWeeks => dateTime.NowDatabaseTime.AddDays(14),
            UserApiTokenTimeframe.OneMonth => dateTime.NowDatabaseTime.AddMonths(1),
            UserApiTokenTimeframe.TwoMonths => dateTime.NowDatabaseTime.AddMonths(2),
            UserApiTokenTimeframe.ThreeMonths => dateTime.NowDatabaseTime.AddMonths(3),
            UserApiTokenTimeframe.SixMonths => dateTime.NowDatabaseTime.AddMonths(6),
            UserApiTokenTimeframe.OneYear => dateTime.NowDatabaseTime.AddYears(1),
            UserApiTokenTimeframe.TwoYears => dateTime.NowDatabaseTime.AddYears(2),
            UserApiTokenTimeframe.ThreeYears => dateTime.NowDatabaseTime.AddYears(3),
            UserApiTokenTimeframe.FiveYears => dateTime.NowDatabaseTime.AddYears(5),
            UserApiTokenTimeframe.TenYears => dateTime.NowDatabaseTime.AddYears(10),
            _ => throw new ArgumentOutOfRangeException(nameof(timeframe), timeframe, null)
        };
    }
}