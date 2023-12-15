namespace Application.Constants;

public class ApiConstants
{
    public static class Monitoring
    {
        public const string Health = "/_health";
    }
    
    public static class GameServer
    {
        public static class Host
        {
            public const string GetRegistration = "/api/gameserver/host/get-registration";
            public const string RegistrationConfirm = "/api/gameserver/host/registration-confirm";
            public const string GetToken = "/api/gameserver/host/get-token";
            public const string CheckIn = "/api/gameserver/host/checkin";
        }
    }
}