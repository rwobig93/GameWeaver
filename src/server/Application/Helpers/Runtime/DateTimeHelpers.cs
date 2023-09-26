﻿namespace Application.Helpers.Runtime;

public static class DateTimeHelpers
{
    public static DateTime ConvertToLocal(this DateTime originalDateTime, TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(originalDateTime, timeZone);
    }

    public static string ToFriendlyDisplay(this DateTime dateTime)
    {
        return dateTime.ToString("MMMM dd yyyy hh:mm:ss tt");
    }

    public static string ToFriendlyDisplayTimezone(this DateTime dateTime)
    {
        return dateTime.ToString("MMMM dd yyyy hh:mm:ss tt zz");
    }

    public static string ToFriendlyDisplayMilitary(this DateTime dateTime)
    {
        return dateTime.ToString("MMMM dd yyyy HH:mm:ss");
    }

    public static string ToFriendlyDisplayMilitaryTimezone(this DateTime dateTime)
    {
        return dateTime.ToString("MMMM dd yyyy HH:mm:ss zz");
    }
}