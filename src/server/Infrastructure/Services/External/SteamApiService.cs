﻿using System.Text.Json;
using Application.Constants.Web;
using Application.Helpers.External;
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

    public async Task<IResult<IEnumerable<SteamApiAppResponseJson>>> GetAllApps()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(ApiConstants.Clients.SteamApiPoweredComUnauthenticated);
            var response = await httpClient.GetAsync(ApiConstants.Steam.ApiAppList);
            if (!response.IsSuccessStatusCode)
            {
                return await Result<IEnumerable<SteamApiAppResponseJson>>.FailAsync($"Error: [{response.StatusCode}]{response.ReasonPhrase}");
            }

            var convertedResponse = _serializerService.DeserializeJson<SteamApiAppListRootResponseJson>(await response.Content.ReadAsStringAsync());
            return await Result<IEnumerable<SteamApiAppResponseJson>>.SuccessAsync(convertedResponse.AppList.Apps);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<SteamApiAppResponseJson>>.FailAsync($"SteamApiAppList Error: {ex.Message}");
        }
    }

    public async Task<IResult<SteamAppInfo?>> GetCurrentAppBuild(int appId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(ApiConstants.Clients.SteamApiNetUnauthenticated);
            var response = await httpClient.GetAsync(ApiConstants.Steam.ApiAppInfo(appId));
            if (!response.IsSuccessStatusCode)
            {
                return await Result<SteamAppInfo?>.FailAsync($"Error: [{response.StatusCode}]{response.ReasonPhrase}");
            }
            
            var convertedResponse = _serializerService.DeserializeJson<SteamApiResponseJson>(await response.Content.ReadAsStringAsync());

            var appIdRoot = convertedResponse.Data.GetNestedValue(appId.ToString());
            if (appIdRoot is null)
            {
                return await Result<SteamAppInfo?>.FailAsync($"App Id '{appId}' doesn't exist in the data payload");
            }

            var isValidAppId = int.TryParse(((JsonElement) appIdRoot).GetNestedValue("appid").ToString(), out var parsedAppId);
            if (!isValidAppId)
            {
                return await Result<SteamAppInfo?>.FailAsync("Unable to parse App Id from payload");
            }
            
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

                var isValidLong = long.TryParse(timeUpdatedRaw, out var convertedEpochTime);
                if (isValidLong)
                {
                    appInfo.LastUpdatedUtc = DateTimeOffset.FromUnixTimeSeconds(convertedEpochTime).UtcDateTime;
                }
            }

            if (string.IsNullOrWhiteSpace(appInfo.VersionBuild))
            {
                return await Result<SteamAppInfo?>.FailAsync("Build version received from the API is empty");
            }

            return await Result<SteamAppInfo?>.SuccessAsync(appInfo);
        }
        catch (Exception ex)
        {
            return await Result<SteamAppInfo?>.FailAsync($"Failure occurred getting app version: {ex.Message}");
        }
    }

    public async Task<IResult<SteamAppDetailResponseJson?>> GetAppDetail(int appId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(ApiConstants.Clients.SteamStoreUnauthenticated);
            var response = await httpClient.GetAsync(ApiConstants.Steam.StoreAppDetails(appId));
            if (!response.IsSuccessStatusCode)
            {
                return await Result<SteamAppDetailResponseJson?>.FailAsync($"Error: [{response.StatusCode}]{response.ReasonPhrase}");
            }

            var convertedResponse = _serializerService.ParseSteamAppDetailJson(await response.Content.ReadAsStringAsync());
            if (convertedResponse is null)
            {
                return await Result<SteamAppDetailResponseJson?>.FailAsync("Failed to parse app detail response");
            }
            
            return await Result<SteamAppDetailResponseJson?>.SuccessAsync(convertedResponse);
        }
        catch (Exception ex)
        {
            return await Result<SteamAppDetailResponseJson?>.FailAsync($"SteamAppDetail Error: {ex.Message}");
        }
    }
}