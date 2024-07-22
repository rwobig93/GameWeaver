using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Requests.GameServer.LocalResource;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

public static class LocalResourceEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsLocalResource(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.GetAllPaginated, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.GetByGameProfileId, GetByGameProfileId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.GetForGameServerId, GetForGameServerId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.LocalResource.Create, Create).ApiVersionOne();
        app.MapPatch(ApiRouteConstants.GameServer.LocalResource.Update, Update).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.LocalResource.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.Search, Search).ApiVersionOne();
    }
    
    /// <summary>
    /// Get all local resources with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of local resources</returns>
    [Authorize(PermissionConstants.GameServer.LocalResource.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<LocalResourceSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await gameServerService.GetAllLocalResourcesPaginatedAsync(pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<LocalResourceSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.LocalResource.GetAllPaginated, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.LocalResource.GetAllPaginated, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get total local resources count
    /// </summary>
    /// <param name="gameServerService"></param>
    /// <returns>Count of total resources</returns>
    [Authorize(PermissionConstants.GameServer.LocalResource.GetCount)]
    private static async Task<IResult<int>> GetCount(IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetLocalResourcesCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a local resource by id
    /// </summary>
    /// <param name="id">Local resource id</param>
    /// <param name="gameServerService"></param>
    /// <returns>Local resource object</returns>
    [Authorize(PermissionConstants.GameServer.LocalResource.Get)]
    private static async Task<IResult<LocalResourceSlim>> GetById([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetLocalResourceByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<LocalResourceSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get local resources by game profile id
    /// </summary>
    /// <param name="id">Game profile id</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of local resources</returns>
    [Authorize(PermissionConstants.GameServer.LocalResource.Get)]
    private static async Task<IResult<IEnumerable<LocalResourceSlim>>> GetByGameProfileId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetLocalResourcesByGameProfileIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get local resources by game server id
    /// </summary>
    /// <param name="id">Game server id</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of local resources</returns>
    [Authorize(PermissionConstants.GameServer.LocalResource.GetForGameServerId)]
    private static async Task<IResult<IEnumerable<LocalResourceSlim>>> GetForGameServerId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetLocalResourcesForGameServerIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create a local resource
    /// </summary>
    /// <param name="request"></param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created local resource</returns>
    [Authorize(PermissionConstants.GameServer.LocalResource.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]LocalResourceCreateRequest request, IGameServerService gameServerService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.CreateLocalResourceAsync(request, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Update a local resources properties
    /// </summary>
    /// <param name="request">Required properties to update a local resource</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.LocalResource.Update)]
    private static async Task<IResult> Update([FromBody]LocalResourceUpdateRequest request, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.UpdateLocalResourceAsync(request, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Delete a local resource
    /// </summary>
    /// <param name="id">Local resource id</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.LocalResource.Delete)]
    private static async Task<IResult> Delete([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.DeleteLocalResourceAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Search local resources by properties
    /// </summary>
    /// <param name="searchText">Text to search for</param>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of matching local resources</returns>
    /// <remarks>Searches by: ID, GameProfileId, GameServerId, Name, Path, Extension, Args</remarks>
    [Authorize(PermissionConstants.GameServer.LocalResource.Search)]
    private static async Task<IResult<IEnumerable<LocalResourceSlim>>> Search([FromQuery]string searchText, [FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await gameServerService.SearchLocalResourcePaginatedAsync(searchText, pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<LocalResourceSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.LocalResource.Search, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.LocalResource.Search, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ex.Message);
        }
    }
}