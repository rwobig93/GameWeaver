using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.Host;
using Application.Requests.GameServer.Host;
using Application.Responses.v1.GameServer;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

/// <summary>
/// Endpoints dealing with game server host operations
/// </summary>
public static class HostEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsHost(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRouteConstants.GameServer.Host.GetToken, GetToken).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Host.GetAllPaginated, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Host.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Host.GetByHostname, GetByHostname).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Host.Create, Create).ApiVersionOne();
        app.MapPatch(ApiRouteConstants.GameServer.Host.Update, Update).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Host.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Host.Search, Search).ApiVersionOne();
    }

    /// <summary>
    /// Generate a valid authorization token for use in host API endpoints
    /// </summary>
    /// <param name="request">Valid and active Host ID and Host Key</param>
    /// <param name="hostService"></param>
    /// <returns>JWT with an expiration datetime in GMT/UTC</returns>
    /// <remarks>
    /// - Expiration time returned is in GMT/UTC
    /// </remarks>
    [AllowAnonymous]
    private static async Task<IResult<HostAuthResponse>> GetToken([FromBody]HostAuthRequest request, IHostService hostService)
    {
        try
        {
            return await hostService.GetToken(request);
        }
        catch (Exception ex)
        {
            return await Result<HostAuthResponse>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get all game server hosts with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="hostService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of game server hosts</returns>
    [Authorize(PermissionConstants.GameServer.Hosts.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<HostSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize, IHostService hostService, 
        IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await hostService.GetAllPaginatedAsync(pageNumber, pageSize) as PaginatedResult<IEnumerable<HostSlim>>;
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<HostSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Host.GetAllPaginated, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Host.GetAllPaginated, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a host by it's id
    /// </summary>
    /// <param name="id">Id of the host to retrieve</param>
    /// <param name="hostService"></param>
    /// <returns>Host object</returns>
    [Authorize(PermissionConstants.GameServer.Hosts.Get)]
    private static async Task<IResult<HostSlim>> GetById([FromQuery]Guid id, IHostService hostService)
    {
        try
        {
            return await hostService.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<HostSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a host by it's hostname
    /// </summary>
    /// <param name="hostname"></param>
    /// <param name="hostService"></param>
    /// <returns>Host object</returns>
    [Authorize(PermissionConstants.GameServer.Hosts.Get)]
    private static async Task<IResult<HostSlim>> GetByHostname([FromQuery]string hostname, IHostService hostService)
    {
        try
        {
            return await hostService.GetByHostnameAsync(hostname);
        }
        catch (Exception ex)
        {
            return await Result<HostSlim>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create a host
    /// </summary>
    /// <param name="request">Required properties request to create a host</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>ID of the host that was created</returns>
    [Authorize(PermissionConstants.GameServer.Hosts.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]HostCreateRequest request, IHostService hostService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await hostService.CreateAsync(request, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Update the properties of a host
    /// </summary>
    /// <param name="request">Required properties to update a host</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with any context messages</returns>
    [Authorize(PermissionConstants.GameServer.Hosts.Update)]
    private static async Task<IResult> Update([FromBody]HostUpdateRequest request, IHostService hostService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await hostService.UpdateAsync(request, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Delete a host by it's id
    /// </summary>
    /// <param name="id">Id of the host to delete</param>
    /// <param name="hostService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Hosts.Delete)]
    private static async Task<IResult> Delete([FromQuery]Guid id, IHostService hostService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUser = await currentUserService.GetApiCurrentUserBasic();
            return await hostService.DeleteAsync(id, currentUser.Id);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Search for a host by properties
    /// </summary>
    /// <param name="searchText">Text to search by</param>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="hostService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of hosts matching the search criteria</returns>
    /// <remarks>Search matches against ID, FriendlyName, Description, PrivateIp and PublicIp</remarks>
    [Authorize(PermissionConstants.GameServer.Hosts.Search)]
    private static async Task<IResult<IEnumerable<HostSlim>>> Search([FromQuery]string searchText, [FromQuery]int pageNumber, [FromQuery]int pageSize,
        IHostService hostService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await hostService.SearchPaginatedAsync(searchText, pageNumber, pageSize) as PaginatedResult<IEnumerable<HostSlim>>;
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<HostSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Host.Search, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Host.Search, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<HostSlim>>.FailAsync(ex.Message);
        }
    }
}