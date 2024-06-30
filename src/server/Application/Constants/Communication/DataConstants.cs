namespace Application.Constants.Communication;

public static class DataConstants
{
    public static class MimeTypes
    {
        public const string OpenXml = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string Binary = "application/octet-stream";
    }

    public static class DateTime
    {
        public const string DisplayFormat = "MM/dd/yyyy hh:mm:ss tt zzz";
        public const string TimeShortFormat = "hh:mm:ss";
        public const string FileNameFormat = "ddMMyyyyHHmmss";
    }

    public static class Logging
    {
        public const string JobDailyCleanup = "DAILY_CLEANUP ::";
    }
}