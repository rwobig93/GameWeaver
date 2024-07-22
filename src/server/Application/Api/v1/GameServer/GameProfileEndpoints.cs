using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Requests.GameServer.GameProfile;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

public static class GameProfileEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsGameProfile(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetAll, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetByFriendlyName, GetByFriendlyName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetByGameId, GetByGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetByOwnerId, GetByOwnerId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.GameProfile.Create, Create).ApiVersionOne();
        app.MapPatch(ApiRouteConstants.GameServer.GameProfile.Update, Update).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.GameProfile.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.Search, Search).ApiVersionOne();
    }
            
    /// <summary>
    /// Get all game profiles with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of game profiles</returns>
    [Authorize(PermissionConstants.GameServer.GameProfile.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<GameProfileSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await gameServerService.GetAllGameProfilesPaginatedAsync(pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<GameProfileSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.GameProfile.GetAll, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.GameProfile.GetAll, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get total game profile count
    /// </summary>
    /// <param name="gameServerService"></param>
    /// <returns>Count of total game profiles</returns>
    [Authorize(PermissionConstants.GameServer.GameProfile.GetCount)]
    private static async Task<IResult<int>> GetCount(IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetGameProfileCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game profile by id
    /// </summary>
    /// <param name="id">Game profile Id</param>
    /// <param name="gameServerService"></param>
    /// <returns>Game profile object</returns>
    [Authorize(PermissionConstants.GameServer.GameProfile.Get)]
    private static async Task<IResult<GameProfileSlim>> GetById([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetGameProfileByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameProfileSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game profile by friendly name
    /// </summary>
    /// <param name="friendlyName">Game profile friendly name</param>
    /// <param name="gameServerService"></param>
    /// <returns>Game profile object</returns>
    [Authorize(PermissionConstants.GameServer.GameProfile.Get)]
    private static async Task<IResult<GameProfileSlim>> GetByFriendlyName([FromQuery]string friendlyName, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetGameProfileByFriendlyNameAsync(friendlyName);
        }
        catch (Exception ex)
        {
            return await Result<GameProfileSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get list of game profiles by game id
    /// </summary>
    /// <param name="id">Game id</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of game profiles</returns>
    [Authorize(PermissionConstants.GameServer.GameProfile.Get)]
    private static async Task<IResult<IEnumerable<GameProfileSlim>>> GetByGameId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetGameProfilesByGameIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get game profiles by owner id
    /// </summary>
    /// <param name="id">Owner account id</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of game profiles</returns>
    [Authorize(PermissionConstants.GameServer.GameProfile.Get)]
    private static async Task<IResult<IEnumerable<GameProfileSlim>>> GetByOwnerId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetGameProfilesByOwnerIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Create a game profile
    /// </summary>
    /// <param name="request">Required properties to create a game profile</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created game profile</returns>
    [Authorize(PermissionConstants.GameServer.GameProfile.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]GameProfileCreateRequest request, IGameServerService gameServerService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.CreateGameProfileAsync(request, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Update a game profiles properties
    /// </summary>
    /// <param name="request">Required properties to update a game profile</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.GameProfile.Update)]
    private static async Task<IResult> Update([FromBody]GameProfileUpdateRequest request, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.UpdateGameProfileAsync(request, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Delete a game profile
    /// </summary>
    /// <param name="id">Game profile id</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.GameProfile.Delete)]
    private static async Task<IResult> Delete([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.DeleteGameProfileAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Search game profiles by properties
    /// </summary>
    /// <param name="searchText">Text to search for</param>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of matching game profiles</returns>
    /// <remarks>Searches by: ID, FriendlyName, OwnerId, GameId, ServerProcessName</remarks>
    [Authorize(PermissionConstants.GameServer.GameProfile.Search)]
    private static async Task<IResult<IEnumerable<GameProfileSlim>>> Search([FromQuery]string searchText, [FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await gameServerService.SearchGameProfilesPaginatedAsync(searchText, pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<GameProfileSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.GameProfile.Search, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.GameProfile.Search, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(ex.Message);
        }
    }
}