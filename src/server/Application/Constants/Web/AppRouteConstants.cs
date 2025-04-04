namespace Application.Constants.Web;

public static class AppRouteConstants
{
    public const string Index = "/";

    public static class Example
    {
        public const string Counter = "/example/counter";
        public const string WeatherData = "/example/weather-data";
        public const string Books = "/example/books";
    }

    public static class Identity
    {
        public const string Login = "/identity/login";
        public const string Register = "/identity/register";
        public const string ConfirmEmail = "/identity/confirm-email";
        public const string ForgotPassword = "/identity/reset-password";
    }

    public static class Account
    {
        public const string AccountSettings = "/account/settings";
        public const string Themes = "/account/themes";
        public const string Security = "/account/security";
    }

    public static class Admin
    {
        public const string Users = "/admin/users";
        public const string UserView = "/admin/user/view";
        public const string Roles = "/admin/roles";
        public const string RoleView = "/admin/role/view";
        public const string AuditTrails = "/admin/audittrail";
        public const string AuditTrailView = "/admin/audittrail/view";
        public const string ServiceAccountAdmin = "/admin/service/users";
        public const string TroubleshootRecords = "/admin/troubleshoot";
        public const string TroubleshootRecordsView = "/admin/troubleshoot/view";
    }

    public static class Developer
    {
        public const string Testing = "/dev/testing";
    }

    public static class Api
    {
        public const string Root = "/api";
    }

    public static class Jobs
    {
        public const string Root = "/jobs";
    }

    public static class GameServer
    {
        public static class Hosts
        {
            public const string HostsDashboard = "/hosts/dashboard";
            public const string View = "/hosts/{HostId:guid}";
            public static string ViewId (Guid id) => $"/hosts/{id}";
        }

        public static class Games
        {
            public const string ViewAll = "/games";
            public const string View = "/games/{GameId:guid}";
            public static string ViewId (Guid id) => $"/games/{id}";
        }

        public static class GameProfiles
        {
            public const string ViewAll = "/gameprofiles";
            public const string View = "/gameprofiles/{GameProfileId:guid}";
            public static string ViewId (Guid id) => $"/gameprofiles/{id}";
        }

        public static class GameServers
        {
            public const string ViewAll = "/gameservers";
            public const string View = "/gameservers/{GameServerId:guid}";
            public static string ViewId (Guid id) => $"/gameservers/{id}";
        }
    }
}