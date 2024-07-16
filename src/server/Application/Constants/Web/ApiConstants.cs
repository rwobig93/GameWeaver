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
        public const string SteamApiNetUnauthenticated = "Steam-ApiNet-Unauthenticated";
        public const string SteamApiPoweredComUnauthenticated = "Steam-ApiPoweredCom-Unauthenticated";
        public const string SteamStoreAuthenticated = "Steam-Store-Authenticated";
        public const string SteamStoreUnauthenticated = "Steam-Store-Unauthenticated";
    }

    public static class Steam
    {
        public const string BaseUrlApiNet = "https://api.steamcmd.net";
        public const string BaseUrlApiPoweredCom = "https://api.steampowered.com";
        public const string BaseUrlStore = "https://store.steampowered.com";
        public static string ApiAppList => "/ISteamApps/GetAppList/v2/?";
        public static string ApiServersAtAddress(string ip) => $"/ISteamApps/GetServersAtAddress/v1/?addr={ip}";
        
        public static string ApiAppInfo(int appId) => $"/v1/info/{appId}";
        
        public static string StoreAppList(string apiKey, int lastAppId, int maxResults = 50000)
        {
            return $"/IStoreService/GetAppList/v1/?key={apiKey}&include_games=true&include_dlc=false&include_software=false&include_videos=false&" +
                   $"include_hardware=false&last_appid={lastAppId}&max_results={maxResults}";
        }
        
        public static string StoreAppDetails(int appId) => $"/api/appdetails?appids={appId}";
    }
}