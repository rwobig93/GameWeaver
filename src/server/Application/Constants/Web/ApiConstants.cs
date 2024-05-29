using Asp.Versioning;
using Asp.Versioning.Builder;

namespace Application.Constants.Web;

public static class ApiConstants
{
    public static ApiVersionSet? SupportsVersionOne { get; set; }
    public static ApiVersionSet? SupportsOneTwo { get; set; }
    public static readonly ApiVersion Version1 = new(1.0);
    public static readonly ApiVersion Version2 = new(2.0);
    
    public static class Clients
    {
        public const string GameWeaverDefault = "Default";
        public const string SteamUnauthenticated = "Steam-Unauthenticated";
        public const string SteamAuthenticated = "Steam-Authenticated";
    }

    public static class Steam
    {
        public const string BaseUrl = "api.steamcmd.net";
        public static string AppInfo(int appId) => $"/v1/info/{appId}";
    }
}