using System.Text;
using Application.Constants;
using Application.Helpers;
using Application.Requests.Host;
using Application.Responses.Host;
using Application.Responses.Monitoring;
using Application.Services;
using Application.Settings;
using Domain.Contracts;
using Domain.Models.Host;
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
            var httpClient = _httpClientFactory.CreateClient(HttpConstants.AuthenticatedServer);
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
        var hostIdIsValid = Guid.TryParse(_authConfig.CurrentValue.Host, out _);
        if (string.IsNullOrWhiteSpace(_authConfig.CurrentValue.RegisterUrl) && hostIdIsValid && !string.IsNullOrWhiteSpace(_authConfig.CurrentValue.Key))
            return await Result<HostRegisterResponse>.SuccessAsync();
        if (!_authConfig.CurrentValue.RegisterUrl.StartsWith(_generalConfig.ServerUrl))
            return await Result<HostRegisterResponse>.FailAsync("Register URL in settings is invalid, please fix the URL and try again");

        // Prep confirmation request
        var confirmRequest = ApiHelpers.GetRequestFromUrl(_authConfig.CurrentValue.RegisterUrl);
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.AuthenticatedServer);
        var payload = new StringContent(_serializerService.SerializeJson(confirmRequest), Encoding.UTF8, "application/json");

        // Handle registration confirmation to the control server
        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.RegistrationConfirm, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        var convertedResponse = _serializerService.DeserializeJson<HostRegisterResponse>(responseContent);
        if (!response.IsSuccessStatusCode || !convertedResponse.Succeeded)
            return await Result<HostRegisterResponse>.FailAsync(convertedResponse.Messages);

        // Parse the successful registration, clear URL in settings and save HostId and Key for authenticating to the control server going forward
        var updatedAuthConfig = new AuthConfiguration
        {
            RegisterUrl = "",
            Host = convertedResponse.Data.HostId.ToString(),
            Key = convertedResponse.Data.HostToken,
            TokenRenewThresholdMinutes = _authConfig.CurrentValue.TokenRenewThresholdMinutes
        };
        var saveResponse = await _serializerService.SaveSettings(AuthConfiguration.SectionName, updatedAuthConfig);
        if (!saveResponse.Succeeded)
            return await Result<HostRegisterResponse>.FailAsync(saveResponse.Messages);

        return await Result<HostRegisterResponse>.SuccessAsync(convertedResponse);
    }

    /// <summary>
    /// Get a new auth token from the control server using the host and key pair retrieved from registration
    /// </summary>
    /// <remarks>Will update the 'ActiveToken' property of this service and add the token to the active httpClient</remarks>
    /// <returns>Token, Refresh Token and Token Expiration in UTC/GMT</returns>
    public async Task<IResult<HostAuthResponse>> GetToken()
    {
        var hostIdIsValid = Guid.TryParse(_authConfig.CurrentValue.Host, out var parsedHostId);
        if (!hostIdIsValid || string.IsNullOrWhiteSpace(_authConfig.CurrentValue.Key))
            return await Result<HostAuthResponse>.FailAsync("HostId or key is invalid, please fix the host/key pair or do a new registration");
        
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
            return await Result<HostAuthResponse>.FailAsync(convertedResponse.Messages);

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
        if (expirationTime.Minutes > expirationThreshold.Minutes && !string.IsNullOrWhiteSpace(Authorization.Token))
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
    public async Task<IResult<byte[]>> Checkin(HostCheckInRequest request)
    {
        if (!RegisteredWithServer) { return await Result<byte[]>.SuccessAsync(); }
        
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.AuthenticatedServer);
        var payload = new ByteArrayContent(_serializerService.SerializeMemory(request));

        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.CheckIn, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return await Result<byte[]>.FailAsync(responseContent);

        return await Result<byte[]>.SuccessAsync();
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
        var payload = new ByteArrayContent(_serializerService.SerializeMemory(request));

        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.UpdateWorkStatus, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return await Result.FailAsync(responseContent);

        return await Result.SuccessAsync();
    }
}