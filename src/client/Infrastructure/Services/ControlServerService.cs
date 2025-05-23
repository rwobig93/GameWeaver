﻿using System.Text;
using Application.Constants;
using Application.Helpers;
using Application.Requests.Host;
using Application.Responses.Game;
using Application.Responses.Host;
using Application.Responses.Monitoring;
using Application.Services;
using Application.Settings;
using Domain.Contracts;
using Domain.Models.ControlServer;
using Domain.Models.Host;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class ControlServerService : IControlServerService
{
    private readonly IOptionsMonitor<AuthConfiguration> _authConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISerializerService _serializerService;
    private readonly GeneralConfiguration _generalConfig;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger _logger;

    public ControlServerService(IOptionsMonitor<AuthConfiguration> authConfig, IHttpClientFactory httpClientFactory, ISerializerService serializerService,
        IOptions<GeneralConfiguration> generalConfig, IDateTimeService dateTimeService, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _serializerService = serializerService;
        _dateTimeService = dateTimeService;
        _logger = logger;
        _generalConfig = generalConfig.Value;
        _authConfig = authConfig;
    }

    public bool ServerIsUp { get; private set; }
    public bool RegisteredWithServer { get; private set; }
    public HostAuthorization Authorization { get; } = new() { Token = "" };

    /// <summary>
    /// Check if the control server is up and responding
    /// </summary>
    /// <remarks>Will update the 'ServerIsUp' property of this service</remarks>
    /// <returns>Boolean indicating if the control server is up</returns>
    public async Task<bool> CheckIfServerIsUp()
    {
        try
        {
            // Hit the health monitoring endpoint of the control server to validate the server is up
            var httpClient = _httpClientFactory.CreateClient(HttpConstants.Unauthenticated);
            var response = await httpClient.GetAsync(ApiConstants.Monitoring.Health);
            ServerIsUp = response.IsSuccessStatusCode;
            if (!ServerIsUp)
            {
                _logger.Fatal("Control Server is currently down");
                return ServerIsUp;
            }

            // Since the control server is up we'll parse the response and indicate if the server is reporting an unhealthy state for troubleshooting
            var convertedResponse = _serializerService.DeserializeJson<HealthCheckResponse>(await response.Content.ReadAsStringAsync());
            if (convertedResponse.status != "Healthy")
            {
                _logger.Warning("Control Server is up but is in an unhealthy state");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred attempting to validate control server status: {Error}", ex.Message);
            ServerIsUp = false;
        }

        return ServerIsUp;
    }

    /// <summary>
    /// Confirms registration based on the RegistrationUrl in the AuthConfig section of the settings file
    /// </summary>
    /// <remarks>
    ///   - Register URL is cleared in the settings file when a successful registration completes
    ///   - General configuration ServerURL is updated when a successful registration completes
    ///   - Will always do a new registration confirmation if Register URL is valid and isn't empty
    ///   - Will attempt to get a token if register url is empty
    /// </remarks>
    /// <returns>Host and key pair used to authenticate with the control server</returns>
    public async Task<IResult<HostRegisterResponse?>> RegistrationConfirm()
    {
        // Client should already be registered if there is no register URL, and we have a valid id and Key pair, so we'll attempt to get a token
        if (string.IsNullOrWhiteSpace(_authConfig.CurrentValue.RegisterUrl))
        {
            var tokenResult = await GetToken();
            if (!tokenResult.Succeeded)
            {
                return await Result<HostRegisterResponse>.FailAsync(tokenResult.Messages);
            }

            return await Result<HostRegisterResponse>.SuccessAsync();
        }

        // Prep confirmation request
        var confirmRequest = ApiHelpers.GetRequestFromUrl(_authConfig.CurrentValue.RegisterUrl);
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.AuthenticatedServer);
        var payload = new StringContent(_serializerService.SerializeJson(confirmRequest), Encoding.UTF8, "application/json");

        // Handle registration confirmation to the control server
        var response = await httpClient.PostAsync(ApiConstants.GameServer.HostRegistration.Confirm, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        var convertedResponse = _serializerService.DeserializeJson<HostRegisterResponse>(responseContent);
        if (!response.IsSuccessStatusCode || !convertedResponse.Succeeded)
        {
            return await Result<HostRegisterResponse>.FailAsync(convertedResponse.Messages);
        }

        // Update the server URL in the general configuration to the newly registered control server
        var parsedServerUrl = new Uri(_authConfig.CurrentValue.RegisterUrl);
        var updatedGeneralConfig = new GeneralConfiguration
        {
            ServerUrl = parsedServerUrl.GetLeftPart(UriPartial.Authority),
            CommunicationQueueMaxPerRun = _generalConfig.CommunicationQueueMaxPerRun,
            MaxQueueAttempts = _generalConfig.MaxQueueAttempts,
            ControlServerWorkIntervalMs = _generalConfig.ControlServerWorkIntervalMs,
            HostWorkIntervalMs = _generalConfig.HostWorkIntervalMs,
            GameServerWorkIntervalMs = _generalConfig.GameServerWorkIntervalMs,
            ResourceGatherIntervalMs = _generalConfig.ResourceGatherIntervalMs,
            GameServerStatusCheckIntervalSeconds = _generalConfig.GameServerStatusCheckIntervalSeconds,
            AppDirectory = _generalConfig.AppDirectory,
            SimultaneousQueueWorkCountMax = _generalConfig.SimultaneousQueueWorkCountMax,
            GameserverBackupsToKeep = _generalConfig.GameserverBackupsToKeep,
            GameserverBackupIntervalMinutes = _generalConfig.GameserverBackupIntervalMinutes
        };
        var generalSaveResponse = await _serializerService.SaveSettings(GeneralConfiguration.SectionName, updatedGeneralConfig);
        if (!generalSaveResponse.Succeeded)
        {
            return await Result<HostRegisterResponse>.FailAsync(generalSaveResponse.Messages);
        }

        // Parse the successful registration, clear URL in settings and save HostId and Key for authenticating to the control server going forward
        var updatedAuthConfig = new AuthConfiguration
        {
            RegisterUrl = "",
            Host = convertedResponse.Data.HostId.ToString(),
            Key = convertedResponse.Data.HostToken,
            TokenRenewThresholdMinutes = _authConfig.CurrentValue.TokenRenewThresholdMinutes
        };
        var authSaveResponse = await _serializerService.SaveSettings(AuthConfiguration.SectionName, updatedAuthConfig);
        if (!authSaveResponse.Succeeded)
        {
            return await Result<HostRegisterResponse>.FailAsync(authSaveResponse.Messages);
        }

        return await Result<HostRegisterResponse>.SuccessAsync(convertedResponse);
    }

    /// <summary>
    /// Get a new auth token from the control server using the host and key pair retrieved from registration
    /// </summary>
    /// <remarks>Will update the 'ActiveToken' property of this service and add the token to the active httpClient</remarks>
    /// <returns>Token, Refresh Token and Token Expiration in UTC/GMT</returns>
    public async Task<IResult<HostAuthResponse>> GetToken()
    {
        if (string.IsNullOrWhiteSpace(_authConfig.CurrentValue.Host) && string.IsNullOrWhiteSpace(_authConfig.CurrentValue.Key))
        {
            _logger.Verbose("HostId and key is empty so we haven't registered yet, waiting for registration");
            return await Result<HostAuthResponse>.SuccessAsync();
        }

        var hostIdIsValid = Guid.TryParse(_authConfig.CurrentValue.Host, out var parsedHostId);
        if (!hostIdIsValid || string.IsNullOrWhiteSpace(_authConfig.CurrentValue.Key))
        {
            return await Result<HostAuthResponse>.FailAsync("HostId or key is invalid, please fix the host/key pair or do a new registration");
        }

        // Prep authentication request
        var tokenRequest = new HostAuthRequest
        {
            HostId = parsedHostId,
            HostToken = _authConfig.CurrentValue.Key
        };
        // We must use an unauthenticated client to prevent a stack overflow as the token delegate handler runs with an authenticated client
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.Unauthenticated);
        var payload = new StringContent(_serializerService.SerializeJson(tokenRequest), Encoding.UTF8, "application/json");

        // Handle the token response
        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.GetToken, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        var convertedResponse = _serializerService.DeserializeJson<HostAuthResponse>(responseContent);
        if (!response.IsSuccessStatusCode || !convertedResponse.Succeeded)
        {
            return await Result<HostAuthResponse>.FailAsync(convertedResponse.Messages);
        }

        // Now that we have a successful token response we'll set the active token to be re-used until expiration and indicate we are registered
        Authorization.Token = convertedResponse.Data.Token;
        Authorization.RefreshToken = convertedResponse.Data.RefreshToken;
        Authorization.RefreshTokenExpiryTime = convertedResponse.Data.RefreshTokenExpiryTime;
        RegisteredWithServer = true;

        return await Result<HostAuthResponse>.SuccessAsync(convertedResponse);
    }

    /// <summary>
    /// Shortcut method to ensure the authentication token is valid, will refresh the active token if it is expired or within expiration threshold
    /// </summary>
    /// <returns>Success or Failure, will return a message indicating a failure reason</returns>
    public async Task<IResult> EnsureAuthTokenIsUpdated()
    {
        // Token isn't within or over expiration threshold & we have the authorization token
        var expirationTime = Authorization.RefreshTokenExpiryTime - _dateTimeService.NowDatabaseTime;
        var expirationThreshold = TimeSpan.FromMinutes(_authConfig.CurrentValue.TokenRenewThresholdMinutes);
        if (expirationTime.TotalMinutes >= expirationThreshold.TotalMinutes && !string.IsNullOrWhiteSpace(Authorization.Token))
        {
            return await Result.SuccessAsync();
        }

        // Token is expired or within expiration threshold, so we'll get a new token
        var updateTokenRequest = await GetToken();
        if (!updateTokenRequest.Succeeded)
        {
            return await Result.FailAsync(updateTokenRequest.Messages);
        }

        return await Result.SuccessAsync();
    }

    /// <summary>
    /// Send check-in and host statistics to the control server, also get back any host work to be done
    /// </summary>
    /// <param name="request">Host statistics to be sent to the control server</param>
    /// <returns></returns>
    public async Task<IResult<IEnumerable<WeaverWork>?>> Checkin(HostCheckInRequest request)
    {
        if (!RegisteredWithServer) { return await Result<IEnumerable<WeaverWork>>.SuccessAsync(); }

        var httpClient = _httpClientFactory.CreateClient(HttpConstants.AuthenticatedServer);
        var payload = new StringContent(_serializerService.SerializeJson(request), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiConstants.GameServer.HostCheckins.CheckIn, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return await Result<IEnumerable<WeaverWork>>.FailAsync(new List<WeaverWork>(), responseContent);
        }

        var deserializedResponse = _serializerService.DeserializeJson<HostCheckInResponse>(responseContent);
        return await Result<IEnumerable<WeaverWork>>.SuccessAsync(deserializedResponse.Data);
    }

    /// <summary>
    /// Send status update to the control server for the given work
    /// </summary>
    /// <param name="request">Update request with details regarding the work needing a status update</param>
    /// <returns></returns>
    public async Task<IResult> WorkStatusUpdate(WeaverWorkUpdateRequest request)
    {
        if (!RegisteredWithServer) { return await Result.SuccessAsync(); }

        var httpClient = _httpClientFactory.CreateClient(HttpConstants.AuthenticatedServer);
        var payload = new StringContent(_serializerService.SerializeJson(request), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiConstants.GameServer.WeaverWork.UpdateStatus, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return await Result.FailAsync(responseContent);
        }

        return await Result.SuccessAsync();
    }

    /// <summary>
    /// Download a game server client for a manual sourced game
    /// </summary>
    /// <param name="gameId">ID of the game</param>
    /// <returns>Game server client and download details</returns>
    public async Task<IResult<GameDownload>> DownloadManualClient(Guid gameId)
    {
        if (!RegisteredWithServer) { return await Result<GameDownload>.FailAsync("Not currently registered with a control server"); }

        var httpClient = _httpClientFactory.CreateClient(HttpConstants.AuthenticatedServer);
        var downloadUrl = QueryHelpers.AddQueryString(ApiConstants.GameServer.Game.DownloadLatest, "gameId", gameId.ToString());

        var response = await httpClient.GetAsync(downloadUrl);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return await Result<GameDownload>.FailAsync(responseContent);
        }

        var deserializedResponse = _serializerService.DeserializeJson<GameDownloadResponse?>(responseContent);
        if (deserializedResponse is null)
        {
            return await Result<GameDownload>.FailAsync("Unable to deserialize game download response, payload is corrupt or invalid");
        }

        return await Result<GameDownload>.SuccessAsync(deserializedResponse.Data);
    }
}