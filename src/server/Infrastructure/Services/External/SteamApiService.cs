using System.Text.Json;
using Application.Constants.Web;
using Application.Helpers.Runtime;
using Application.Models.External.Steam;
using Application.Services.External;
using Application.Services.System;
using Domain.Contracts;
using Domain.Enums.GameServer;

namespace Infrastructure.Services.External;

public class SteamApiService : ISteamApiService
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISerializerService _serializerService;

    public SteamApiService(ILogger logger, IHttpClientFactory httpClientFactory, ISerializerService serializerService)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _serializerService = serializerService;
    }

    public async Task<IResult<SteamApiResponse>> GetAllApps()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<SteamAppInfo>> GetCurrentAppVersion(int appId)
    {
        var httpClient = _httpClientFactory.CreateClient(ApiConstants.Clients.SteamUnauthenticated);
        var response = await httpClient.GetAsync(ApiConstants.Steam.AppInfo(appId));
        if (!response.IsSuccessStatusCode)
        {
            return await Result<SteamAppInfo>.FailAsync($"Error: [{response.StatusCode}]{response.ReasonPhrase}");
        }

        try
        {
            var convertedResponse = _serializerService.DeserializeJson<SteamApiResponse>(await response.Content.ReadAsStringAsync());

            var appIdRoot = convertedResponse.data.GetNestedValue($"data.{appId}");
            if (appIdRoot is null)
            {
                return await Result<SteamAppInfo>.FailAsync($"App Id '{appId}' doesn't exist in the data payload");
            }
            
            var parsedAppId = 0;
            ((JsonElement)appIdRoot).GetNestedValue("appid")?.TryGetInt32(out parsedAppId);
            
            var appInfo = new SteamAppInfo { AppId = parsedAppId };

            var appCommon = ((JsonElement) appIdRoot).GetNestedValue("common");
            if (appCommon is not null)
            {
                appInfo.Name = ((JsonElement)appCommon).GetNestedValue("name")?.ToString() ?? "";
                var osList = ((JsonElement)appCommon).GetNestedValue("oslist").ToString() ?? "";
                
                if (osList.Contains("windows"))
                    appInfo.OsSupport.Add(OsType.Windows);
                if (osList.Contains("linux"))
                    appInfo.OsSupport.Add(OsType.Linux);
                if (osList.Contains("mac"))
                    appInfo.OsSupport.Add(OsType.Mac);
            }
            
            var appPublicBranch = ((JsonElement) appIdRoot).GetNestedValue("depots.branches.public");
            if (appPublicBranch is not null)
            {
                appInfo.VersionBuild = ((JsonElement)appPublicBranch).GetNestedValue("buildid")?.ToString() ?? "";
                var timeUpdatedRaw = ((JsonElement)appPublicBranch).GetNestedValue("timeupdated")?.ToString() ?? "";

                long.TryParse(timeUpdatedRaw, out var convertedEpochTime);
                appInfo.LastUpdatedUtc = DateTimeOffset.FromUnixTimeSeconds(convertedEpochTime).UtcDateTime;
            }

            return await Result<SteamAppInfo>.SuccessAsync(appInfo);
        }
        catch (Exception ex)
        {
            return await Result<SteamAppInfo>.FailAsync($"Failure occurred getting app version: {ex.Message}");
        }
    }
}