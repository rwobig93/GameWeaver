using System.Text;
using Application.Constants;
using Application.Helpers;
using Application.Requests.Host;
using Application.Responses.Host;
using Application.Responses.Monitoring;
using Application.Services;
using Application.Services.System;
using Application.Settings;
using Domain.Contracts;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class ControlServerService : IControlServerService
{
    private readonly AuthConfiguration _authConfig;
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
        _authConfig = authConfig.CurrentValue;
    }

    public bool ServerIsUp { get; private set; }
    public HostAuthResponse ActiveToken { get; } = new() { Token = "" };

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
            var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
            var response = await httpClient.GetAsync(ApiConstants.Monitoring.Health);
            ServerIsUp = response.IsSuccessStatusCode;
            if (!ServerIsUp)
            {
                _logger.Fatal("Control Server is currently down");
                return ServerIsUp;
            }

            // Since the control server is up we'll parse the response and indicate if the server is reporting an unhealthy state for troubleshooting
            var convertedResponse = _serializerService.Deserialize<HealthCheckResponse>(await response.Content.ReadAsStringAsync());
            if (convertedResponse.status != "Healthy")
                _logger.Warning("Control Server is up but is in an unhealthy state");
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
    ///   - Will always do a new registration confirmation if Register URL is valid and isn't empty
    /// </remarks>
    /// <returns>Host and key pair used to authenticate with the control server</returns>
    public async Task<IResult<HostRegisterResponse>> RegistrationConfirm()
    {
        // Client should already be registered if there is no register URL and we have a valid Id and Key pair
        var hostIdIsValid = Guid.TryParse(_authConfig.Host, out _);
        if (string.IsNullOrWhiteSpace(_authConfig.RegisterUrl) && hostIdIsValid && !string.IsNullOrWhiteSpace(_authConfig.Key))
            return await Result<HostRegisterResponse>.SuccessAsync();
        if (!_authConfig.RegisterUrl.StartsWith(_generalConfig.ServerUrl))
            return await Result<HostRegisterResponse>.FailAsync("Register URL in settings is invalid, please fix the URL and try again");

        // Prep confirmation request
        var confirmRequest = ApiHelpers.GetRequestFromUrl(_authConfig.RegisterUrl);
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
        var payload = new StringContent(_serializerService.Serialize(confirmRequest), Encoding.UTF8, "application/json");

        // Handle registration confirmation to the control server
        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.RegistrationConfirm, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return await Result<HostRegisterResponse>.FailAsync(responseContent);

        // Parse the successful registration, clear URL in settings and save HostId and Key for authenticating to the control server going forward
        var convertedResponse = _serializerService.Deserialize<HostRegisterResponse>(responseContent);
        var updatedAuthConfig = new AuthConfiguration
        {
            RegisterUrl = "",
            Host = convertedResponse.HostId.ToString(),
            Key = convertedResponse.HostToken
        };
        var saveResponse = await _serializerService.SaveSettings(AuthConfiguration.SectionName, updatedAuthConfig);
        if (!saveResponse.Succeeded)
            return await Result<HostRegisterResponse>.FailAsync(saveResponse.Messages);

        return await Result<HostRegisterResponse>.SuccessAsync(convertedResponse);
    }

    /// <summary>
    /// Get a new auth token from the control server using the host and key pair retrieved from registration
    /// </summary>
    /// <remarks>Will update the 'ActiveToken' property of this service</remarks>
    /// <returns></returns>
    public async Task<IResult<HostAuthResponse>> GetToken()
    {
        var hostIdIsValid = Guid.TryParse(_authConfig.Host, out var parsedHostId);
        if (!hostIdIsValid || string.IsNullOrWhiteSpace(_authConfig.Key))
            return await Result<HostAuthResponse>.FailAsync("HostId or key is invalid, please fix the pair or do a new registration");
        
        // Prep authentication request
        var tokenRequest = new HostAuthRequest
        {
            HostId = parsedHostId,
            HostToken = _authConfig.Key
        };
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
        var payload = new StringContent(_serializerService.Serialize(tokenRequest), Encoding.UTF8, "application/json");

        // Handle the token response
        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.GetToken, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return await Result<HostAuthResponse>.FailAsync(responseContent);

        // Now that we have a successful token response we'll set the active token to be re-used until expiration
        var convertedResponse = _serializerService.Deserialize<HostAuthResponse>(responseContent);
        ActiveToken.Token = convertedResponse.Token;
        ActiveToken.RefreshToken = convertedResponse.RefreshToken;
        ActiveToken.RefreshTokenExpiryTime = convertedResponse.RefreshTokenExpiryTime;
        
        return await Result<HostAuthResponse>.SuccessAsync(convertedResponse);
    }

    /// <summary>
    /// Shortcut method to ensure the authentication token is valid, will refresh the active token if it is expired or within expiration threshold
    /// </summary>
    /// <returns>Success or Failure, will return a message indicating a failure reason</returns>
    private async Task<IResult> EnsureAuthTokenIsUpdated()
    {
        // Token isn't within or over expiration threshold
        if (_dateTimeService.NowDatabaseTime - ActiveToken.RefreshTokenExpiryTime > TimeSpan.FromMinutes(_authConfig.TokenRenewThresholdMinutes))
        {
            return await Result.SuccessAsync();
        }
        
        // Token is expired or within expiration threshold so we'll get a new token
        var updateTokenRequest = await GetToken();
        if (!updateTokenRequest.Succeeded)
            return await Result.FailAsync(updateTokenRequest.Messages);
        
        return await Result.SuccessAsync();
    }

    /// <summary>
    /// Send check-in and host statistics to the control server, also get back any host work to be done
    /// </summary>
    /// <param name="request">Host statistics to be sent to the control server</param>
    /// <returns></returns>
    public async Task<IResult> Checkin(HostCheckInRequest request)
    {
        var tokenEnforcement = await EnsureAuthTokenIsUpdated();
        if (!tokenEnforcement.Succeeded)
            return await Result.FailAsync(tokenEnforcement.Messages);
        
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
        var payload = new StringContent(_serializerService.Serialize(request), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.GetToken, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return await Result.FailAsync(responseContent);

        return await Result.SuccessAsync();
    }
}