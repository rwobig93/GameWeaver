using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Requests.Api;
using Application.Responses.v1.Api;
using Application.Responses.v1.Identity;
using Application.Services.Identity;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.v1.Api;

/// <summary>
/// Endpoints dealing with overall API integration functionality
/// </summary>
[AllowAnonymous]
public static class ApiEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsApi(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRouteConstants.Api.GetToken, GetToken).ApiVersionOne();
        app.MapGet(ApiRouteConstants.Api.WhoAmI, WhoAmI).ApiVersionOne();
    }

    /// <summary>
    /// Gets a Json Web Token (JWT) for use in API calls
    /// </summary>
    /// <param name="tokenRequest">Credentials to authenticate</param>
    /// <param name="accountService"></param>
    /// <returns>JWT with an expiration datetime in GMT/UTC</returns>
    /// <remarks>
    ///     - Expiration time returned is in GMT/UTC
    /// </remarks>
    [AllowAnonymous]
    private static async Task<IResult<ApiTokenResponse>> GetToken([FromBody]ApiGetTokenRequest tokenRequest, IAppAccountService accountService)
    {
        try
        {
            // TODO: Look into adding claim for API access vs manually passing token from user auth for authorization handling
            var response = await accountService.GetApiAuthToken(tokenRequest); 
            return response;
        }
        catch (Exception ex)
        {
            return await Result<ApiTokenResponse>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Return basic information about the currently authenticated account
    /// </summary>
    /// <param name="currentUserService"></param>
    /// <returns>Basic user information</returns>
    [Authorize]
    private static async Task<IResult<UserBasicResponse>> WhoAmI(ICurrentUserService currentUserService)
    {
        try
        {
            var currentUser = await currentUserService.GetApiCurrentUserBasic();
            return await Result<UserBasicResponse>.SuccessAsync(currentUser);
        }
        catch (Exception ex)
        {
            return await Result<UserBasicResponse>.FailAsync(ex.Message);
        }
    }
}