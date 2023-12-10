namespace Application.Constants;

public class ApiConstants
{
    public static class GameServer
    {
        public static class Host
        {
            public const string GetRegistration = "/api/gameserver/host/get-registration";
            public const string RegistrationConfirm = "/api/gameserver/host/registration-confirm";
            public const string CheckIn = "/api/gameserver/host/checkin";
        }
    }
}