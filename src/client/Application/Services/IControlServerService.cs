using Application.Requests.Api;
using Application.Responses.Api;
using Application.Responses.Identity;
using Domain.Contracts;

namespace Application.Services;

public interface IControlServerService
{
    public bool ServerIsUp { get; }
    public ApiTokenResponse ActiveToken { get; }
    
    // TODO: Fill out the server API endpoints, then do corresponding methods here for interoperability 
    Task<IResult<ApiTokenResponse>> GetApiToken(ApiGetTokenRequest tokenRequest);
    Task<IResult<UserBasicResponse>> GetAuthenticatedHost();
    Task<bool> CheckIfServerIsUp();
}