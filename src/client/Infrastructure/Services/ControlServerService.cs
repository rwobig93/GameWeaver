using Application.Requests.Api;
using Application.Responses.Api;
using Application.Responses.Identity;
using Application.Services;
using Domain.Contracts;

namespace Infrastructure.Services;

public class ControlServerService : IControlServerService
{
    public bool ServerIsUp { get; internal set; }

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