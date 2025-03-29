using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Runtime;
using Application.Helpers.Web;
using Application.Mappers.GameServer;
using Application.Models.GameServer.GameServer;
using Application.Requests.GameServer.GameServer;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

/// <summary>
/// Endpoints dealing with game server operations
/// </summary>
public static class GameServerEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsGameserver(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetAll, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetByServerName, GetByServerName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetByGameId, GetByGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetByGameProfileId, GetByGameProfileId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetByHostId, GetByHostId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetByOwnerId, GetByOwnerId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.Create, Create).ApiVersionOne();
        app.MapPatch(ApiRouteConstants.GameServer.Gameserver.Update, Update).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Gameserver.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.Search, Search).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.ChangeOwnership, ChangeOwnership).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.StartServer, StartServer).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.StopServer, StopServer).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.RestartServer, RestartServer).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.UpdateLocalResource, UpdateLocalResource).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.UpdateAllLocalResources, UpdateAllLocalResources).ApiVersionOne();
    }

    /// <summary>
    /// Get all game servers with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <param name="currentUserService"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<GameServerSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize, IGameServerService gameServerService, 
        IOptions<AppConfiguration> appConfig, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await gameServerService.GetAllPaginatedAsync(pageNumber, pageSize, currentUserId);
            if (!result.Succeeded)
            {
                return await PaginatedResult<IEnumerable<GameServerSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Gameserver.GetAll, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Gameserver.GetAll, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get the total game server count
    /// </summary>
    /// <param name="gameServerService"></param>
    /// <returns>Count of total game servers</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.GetCount)]
    private static async Task<IResult<int>> GetCount(IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get a game server by id
    /// </summary>
    /// <param name="id">ID of the game server</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Game server object</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
    private static async Task<IResult<GameServerSlim?>> GetById([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.GetByIdAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<GameServerSlim>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get a game server by name
    /// </summary>
    /// <param name="serverName">Game server name</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Game server object</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
    private static async Task<IResult<GameServerSlim?>> GetByServerName([FromQuery]string serverName, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.GetByServerNameAsync(serverName, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<GameServerSlim>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get game servers by game id
    /// </summary>
    /// <param name="id">ID of a game</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
    private static async Task<IResult<IEnumerable<GameServerSlim>>> GetByGameId([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.GetByGameIdAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get game servers by game profile id
    /// </summary>
    /// <param name="id">Game profile id</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
    private static async Task<IResult<GameServerSlim?>> GetByGameProfileId([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.GetByGameProfileIdAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<GameServerSlim?>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get game servers by host id
    /// </summary>
    /// <param name="id">Host id</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
    private static async Task<IResult<IEnumerable<GameServerSlim>>> GetByHostId([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.GetByHostIdAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get game servers by the owner id
    /// </summary>
    /// <param name="id">ID of the owner</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
    private static async Task<IResult<IEnumerable<GameServerSlim>>> GetByOwnerId([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.GetByOwnerIdAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create a game server
    /// </summary>
    /// <param name="request">Required properties to create a game server</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>ID of the created game server</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]GameServerCreateRequest request, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.CreateAsync(request.ToCreate(), currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Update a game server's properties
    /// </summary>
    /// <param name="request">Required properties to update a game server</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Update)]
    private static async Task<IResult> Update([FromBody]GameServerUpdateRequest request, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.UpdateAsync(request.ToUpdate(), currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Delete a game server
    /// </summary>
    /// <param name="id">ID of the game server</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Delete)]
    private static async Task<IResult> Delete([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.DeleteAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Search for game servers
    /// </summary>
    /// <param name="searchText">Text to search</param>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <param name="currentUserService"></param>
    /// <returns>List of matching game servers</returns>
    /// <remarks>Searches by: ID, OwnerId, HostId, GameId, GameProfileId, PublicIp, PrivateIp, ExternalHostname, ServerName</remarks>
    [Authorize(PermissionConstants.GameServer.Gameserver.Search)]
    private static async Task<IResult<IEnumerable<GameServerSlim>>> Search([FromQuery]string searchText, [FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await gameServerService.SearchPaginatedAsync(searchText, pageNumber, pageSize, currentUserId);
            if (!result.Succeeded)
            {
                return await PaginatedResult<IEnumerable<GameServerSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Gameserver.Search, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Gameserver.Search, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Change ownership of a game server
    /// </summary>
    /// <param name="gameServerId"></param>
    /// <param name="newOwnerId"></param>
    /// <param name="currentUserService"></param>
    /// <param name="gameServerService"></param>
    /// <param name="userService"></param>
    /// <param name="authorizationService"></param>
    /// <returns></returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.ChangeOwnership)]
    private static async Task<IResult> ChangeOwnership([FromQuery]Guid gameServerId, [FromQuery]Guid newOwnerId, ICurrentUserService currentUserService,
        IGameServerService gameServerService, IAppUserService userService, IAuthorizationService authorizationService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            var currentUser = (await currentUserService.GetCurrentUserPrincipal())!;
            if (!await authorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.ChangeOwnership))
            {
                return await Result.FailAsync(ErrorMessageConstants.Permissions.PermissionError);
            }
            
            var serverResponse = await gameServerService.GetByIdAsync(gameServerId, currentUserId);
            if (!serverResponse.Succeeded || serverResponse.Data is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.GameServers.NotFound);
            }
            
            var userResponse = await userService.GetByIdAsync(newOwnerId);
            if (!userResponse.Succeeded || userResponse.Data is null)
            {
                return await Result.FailAsync(ErrorMessageConstants.Users.UserNotFoundError);
            }
            
            var serverUpdate = serverResponse.Data.ToUpdate();
            serverUpdate.OwnerId = newOwnerId;
            var updateResponse = await gameServerService.UpdateAsync(serverUpdate, currentUserId);
            if (!updateResponse.Succeeded)
            {
                return await Result.FailAsync(updateResponse.Messages);
            }
            
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Start a game server
    /// </summary>
    /// <param name="id">Gameserver Id</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.StartServer)]
    private static async Task<IResult> StartServer([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.StartServerAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Stop a game server
    /// </summary>
    /// <param name="id">Gameserver Id</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.StopServer)]
    private static async Task<IResult> StopServer([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.StopServerAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Restart a game server
    /// </summary>
    /// <param name="id">Gameserver Id</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.RestartServer)]
    private static async Task<IResult> RestartServer([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.RestartServerAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
        
    /// <summary>
    /// Update the local resource for the game server on the host
    /// </summary>
    /// <param name="serverId">Game server id</param>
    /// <param name="resourceId">Local resource id</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns></returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.UpdateLocalResource)]
    private static async Task<IResult> UpdateLocalResource([FromQuery]Guid serverId, [FromQuery]Guid resourceId, IGameServerService gameServerService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.UpdateLocalResourceOnGameServerAsync(serverId, resourceId, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Update all local resources for the game server on the host
    /// </summary>
    /// <param name="serverId">Game server id</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns></returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.UpdateAllLocalResources)]
    private static async Task<IResult> UpdateAllLocalResources([FromQuery]Guid serverId, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.UpdateAllLocalResourcesOnGameServerAsync(serverId, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
}