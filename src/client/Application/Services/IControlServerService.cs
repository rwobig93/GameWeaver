using Application.Requests.Api;
using Application.Responses.Api;
using Application.Responses.Identity;
using Domain.Contracts;

namespace Application.Services;

public interface IControlServerService
{
    public bool ServerIsUp { get; }
    Task<IResult<ApiTokenResponse>> GetApiToken(ApiGetTokenRequest tokenRequest);
    Task<IResult<UserBasicResponse>> GetAuthenticatedHost();
    Task<bool> CheckIfServerIsUp();
}