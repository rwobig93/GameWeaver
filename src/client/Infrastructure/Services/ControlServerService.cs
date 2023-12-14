using System.Text;
using Application.Constants;
using Application.Requests.Host;
using Application.Responses.Host;
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

    public ControlServerService(IOptionsMonitor<AuthConfiguration> authConfig, IHttpClientFactory httpClientFactory, ISerializerService serializerService,
        IOptions<GeneralConfiguration> generalConfig, IDateTimeService dateTimeService)
    {
        _httpClientFactory = httpClientFactory;
        _serializerService = serializerService;
        _dateTimeService = dateTimeService;
        _generalConfig = generalConfig.Value;
        _authConfig = authConfig.CurrentValue;
    }

    public bool ServerIsUp { get; internal set; }
    public HostAuthResponse ActiveToken { get; internal set; } = new() { Token = "" };


    public async Task<bool> CheckIfServerIsUp()
    {
        await Task.CompletedTask;
        ServerIsUp = false;
        return false;
    }

    public async Task<IResult<HostRegisterResponse>> RegistrationConfirm()
    {
        if (!_authConfig.RegisterUrl.StartsWith(_generalConfig.ServerUrl))
            return await Result<HostRegisterResponse>.FailAsync("Register URL in settings is invalid, please fix the URL and try again");

        var confirmRequest = new HostRegisterRequest
        {
            HostId = Guid.Parse(_authConfig.Host),
            Key = _authConfig.Key
        };
        
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
        var payload = new StringContent(_serializerService.Serialize(confirmRequest), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.RegistrationConfirm, payload);
        if (!response.IsSuccessStatusCode)
            return await Result<HostRegisterResponse>.FailAsync(response.ToString());

        var convertedResponse = _serializerService.Deserialize<HostRegisterResponse>(response.ToString());
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
        if (!response.IsSuccessStatusCode)
            return await Result<HostAuthResponse>.FailAsync(response.ToString());

        var convertedResponse = _serializerService.Deserialize<HostAuthResponse>(response.ToString());
        ActiveToken.Token = convertedResponse.Token;
        ActiveToken.RefreshToken = convertedResponse.RefreshToken;
        ActiveToken.RefreshTokenExpiryTime = convertedResponse.RefreshTokenExpiryTime;
        return await Result<HostAuthResponse>.SuccessAsync(convertedResponse);
    }

    private async Task<IResult> EnsureAuthTokenIsUpdated()
    {
        // Token isn't within 5min of expiring or over expiration
        if (_dateTimeService.NowDatabaseTime - ActiveToken.RefreshTokenExpiryTime > TimeSpan.FromMinutes(5))
        {
            return await Result.SuccessAsync();
        }
        
        // Token is expired or within 5min of expiring so we'll get a new token to be safe
        var updateTokenRequest = await GetToken();
        if (!updateTokenRequest.Succeeded)
            return await Result.FailAsync(updateTokenRequest.Messages);
        
        return await Result.SuccessAsync();
    }

    public async Task<IResult> Checkin(HostCheckInRequest request)
    {
        await EnsureAuthTokenIsUpdated();
        
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
        var payload = new StringContent(_serializerService.Serialize(request), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.GetToken, payload);
        if (!response.IsSuccessStatusCode)
            return await Result.FailAsync(response.ToString());

        return await Result.SuccessAsync();
    }
}