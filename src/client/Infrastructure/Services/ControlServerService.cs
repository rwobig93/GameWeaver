using Application.Requests.Api;
using Application.Responses.Api;
using Application.Responses.Identity;
using Application.Services;
using Application.Settings;
using Domain.Contracts;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class ControlServerService : IControlServerService
{
    private readonly AuthConfiguration _authConfig;

    public ControlServerService(IOptionsMonitor<AuthConfiguration> authConfig)
    {
        _authConfig = authConfig.CurrentValue;
    }

    public bool ServerIsUp { get; internal set; }

    public ApiTokenResponse ActiveToken { get; internal set; } = new() { Token = "" };

    public async Task<IResult<ApiTokenResponse>> GetApiToken(ApiGetTokenRequest tokenRequest)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<IResult<UserBasicResponse>> GetAuthenticatedHost()
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<bool> CheckIfServerIsUp()
    {
        await Task.CompletedTask;
        ServerIsUp = false;
        return false;
    }
}