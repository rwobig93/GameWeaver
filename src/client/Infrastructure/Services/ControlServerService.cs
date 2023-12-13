using System.Text;
using Application.Constants;
using Application.Requests.Host;
using Application.Responses.Host;
using Application.Services;
using Application.Settings;
using Domain.Contracts;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class ControlServerService : IControlServerService
{
    private readonly AuthConfiguration _authConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISerializerService _serializerService;

    public ControlServerService(IOptionsMonitor<AuthConfiguration> authConfig, IHttpClientFactory httpClientFactory, ISerializerService serializerService)
    {
        _httpClientFactory = httpClientFactory;
        _serializerService = serializerService;
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

    public async Task<IResult<HostRegisterResponse>> RegistrationConfirm(HostRegisterRequest request)
    {
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
        var payload = new StringContent(_serializerService.Serialize(request), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.RegistrationConfirm, payload);
        if (!response.IsSuccessStatusCode)
            return await Result<HostRegisterResponse>.FailAsync(response.ToString());

        var convertedResponse = _serializerService.Deserialize<HostRegisterResponse>(response.ToString());
        return await Result<HostRegisterResponse>.SuccessAsync(convertedResponse);
    }

    public async Task<IResult<HostAuthResponse>> GetToken(HostAuthRequest request)
    {
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
        var payload = new StringContent(_serializerService.Serialize(request), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.GetToken, payload);
        if (!response.IsSuccessStatusCode)
            return await Result<HostAuthResponse>.FailAsync(response.ToString());

        var convertedResponse = _serializerService.Deserialize<HostAuthResponse>(response.ToString());
        ActiveToken.Token = convertedResponse.Token;
        ActiveToken.RefreshToken = convertedResponse.RefreshToken;
        ActiveToken.RefreshTokenExpiryTime = convertedResponse.RefreshTokenExpiryTime;
        return await Result<HostAuthResponse>.SuccessAsync(convertedResponse);
    }

    public async Task<IResult> Checkin(HostCheckInRequest request)
    {
        // TODO: Handle auth, renew token when old token expires | If expiration is within 5 seconds or more then get a new token
        var httpClient = _httpClientFactory.CreateClient(HttpConstants.IdServer);
        var payload = new StringContent(_serializerService.Serialize(request), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiConstants.GameServer.Host.GetToken, payload);
        if (!response.IsSuccessStatusCode)
            return await Result.FailAsync(response.ToString());

        return await Result.SuccessAsync();
    }
}