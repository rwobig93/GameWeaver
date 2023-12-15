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


    public async Task<bool> CheckIfServerIsUp()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
            var response = await httpClient.GetAsync(ApiConstants.Monitoring.Health);
            ServerIsUp = response.IsSuccessStatusCode;
            if (!ServerIsUp)
            {
                _logger.Fatal("Control Server is currently down");
                return ServerIsUp;
            }

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

    public async Task<IResult<HostRegisterResponse>> RegistrationConfirm()
    {
        // TODO: Validate hostId and key to ensure registered or not then register url
        if (string.IsNullOrWhiteSpace(_authConfig.RegisterUrl))
            return await Result<HostRegisterResponse>.SuccessAsync();
        if (!_authConfig.RegisterUrl.StartsWith(_generalConfig.ServerUrl))
            return await Result<HostRegisterResponse>.FailAsync("Register URL in settings is invalid, please fix the URL and try again");

        var confirmRequest = ApiHelpers.GetRequestFromUrl(_authConfig.RegisterUrl);
        
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
        var payload = new StringContent(_serializerService.Serialize(confirmRequest), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.RegistrationConfirm, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return await Result<HostRegisterResponse>.FailAsync(responseContent);

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

    public async Task<IResult<HostAuthResponse>> GetToken()
    {
        var tokenRequest = new HostAuthRequest
        {
            HostId = Guid.Parse(_authConfig.Host),
            HostToken = _authConfig.Key
        };

        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
        var payload = new StringContent(_serializerService.Serialize(tokenRequest), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.GetToken, payload);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return await Result<HostAuthResponse>.FailAsync(responseContent);

        var convertedResponse = _serializerService.Deserialize<HostAuthResponse>(responseContent);
        ActiveToken.Token = convertedResponse.Token;
        ActiveToken.RefreshToken = convertedResponse.RefreshToken;
        ActiveToken.RefreshTokenExpiryTime = convertedResponse.RefreshTokenExpiryTime;
        return await Result<HostAuthResponse>.SuccessAsync(convertedResponse);
    }

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