namespace Application.Constants;

public class ApiConstants
{
    public const string AuthorizationScheme = "Bearer";
    
    public static class Monitoring
    {
        public const string Health = "/_health";
    }
    
    public static class GameServer
    {
        public static class Host
        {
            public const string GetToken = "/api/host/token";
        }

        public static class HostRegistration
        {
            public const string Confirm = "/api/host-registration/confirm";
        }

        public static class HostCheckins
        {
            public const string CheckIn = "/api/host-checkin/checkin";
        }

        public static class WeaverWork
        {
            public const string UpdateStatus = "/api/weaver/work/status";
        }

        public static class Game
        {
            public const string DownloadLatest = "/api/game/download/latest";
        }
    }
}