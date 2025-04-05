namespace Application.Helpers.Runtime;

public class JobHelpers
{
    public class CronString
    {
        public static string Minutely { get; } = "*/1 * * * *";
        public static string MinuteInterval(int interval)
        {
            return $"*/{interval} * * * *";
        }

        public static string Hourly { get; } = "0 */1 * * *";
        public static string HourInterval(int interval)
        {
            return $"0 */{interval} * * *";
        }

        public static string Daily { get; } = "0 0 */1 * *";
        public static string DayInterval(int interval)
        {
            return $"0 0 */{interval} * *";
        }

        public static string Monthly { get; } = "0 0 1 */1 *";
        public static string MonthInterval(int interval)
        {
            return $"0 0 1 */{interval} *";
        }
    }
}