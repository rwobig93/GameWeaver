using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.Game;
using Application.Requests.GameServer.Game;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

public static class GameEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsGame(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.GameServer.Game.GetAllPaginated, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetBySteamName, GetBySteamName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetByFriendlyName, GetByFriendlyName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetBySteamGameId, GetBySteamGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetBySteamToolId, GetBySteamToolId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Game.Create, Create).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Game.Update, Update).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Game.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.Search, Search).ApiVersionOne();
    }
    
    /// <summary>
    /// Get all games with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of games</returns>
    [Authorize(PermissionConstants.GameServer.Game.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<GameSlim>>> GetAllPaginated(int pageNumber, int pageSize, IGameService gameService,
        IOptions<AppConfiguration> appConfig)
    {
        try
        {
            if (pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize) pageSize = 500;

            var result = await gameService.GetAllPaginatedAsync(pageNumber, pageSize);
            if (!result.Succeeded)
                return await Result<IEnumerable<GameSlim>>.FailAsync(result.Messages);
            
            var totalCountRequest = await gameService.GetCountAsync();
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Game.GetAllPaginated,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Game.GetAllPaginated,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<GameSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get total game count
    /// </summary>
    /// <param name="gameService"></param>
    /// <returns>Count of total games</returns>
    [Authorize(PermissionConstants.GameServer.Game.GetCount)]
    private static async Task<IResult<int>> GetCount(IGameService gameService)
    {
        try
        {
            return await gameService.GetCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game by id
    /// </summary>
    /// <param name="id">Game Id</param>
    /// <param name="gameService"></param>
    /// <returns>Game object</returns>
    [Authorize(PermissionConstants.GameServer.Game.Get)]
    private static async Task<IResult<GameSlim?>> GetById(Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameSlim?>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game by steam name
    /// </summary>
    /// <param name="steamName">Game steam name</param>
    /// <param name="gameService"></param>
    /// <returns>Game object</returns>
    [Authorize(PermissionConstants.GameServer.Game.Get)]
    private static async Task<IResult<IEnumerable<GameSlim>>> GetBySteamName(string steamName, IGameService gameService)
    {
        try
        {
            return await gameService.GetBySteamNameAsync(steamName);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game by friendly name
    /// </summary>
    /// <param name="friendlyName">Game friendly name</param>
    /// <param name="gameService"></param>
    /// <returns>Game object</returns>
    [Authorize(PermissionConstants.GameServer.Game.Get)]
    private static async Task<IResult<GameSlim?>> GetByFriendlyName(string friendlyName, IGameService gameService)
    {
        try
        {
            return await gameService.GetByFriendlyNameAsync(friendlyName);
        }
        catch (Exception ex)
        {
            return await Result<GameSlim?>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game by steam id
    /// </summary>
    /// <param name="id">Game steam id</param>
    /// <param name="gameService"></param>
    /// <returns>Game object</returns>
    [Authorize(PermissionConstants.GameServer.Game.Get)]
    private static async Task<IResult<GameSlim?>> GetBySteamGameId(int id, IGameService gameService)
    {
        try
        {
            return await gameService.GetBySteamGameIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameSlim?>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game by steam tool id
    /// </summary>
    /// <param name="id">Game steam tool id</param>
    /// <param name="gameService"></param>
    /// <returns>Game object</returns>
    [Authorize(PermissionConstants.GameServer.Game.Get)]
    private static async Task<IResult<GameSlim?>> GetBySteamToolId(int id, IGameService gameService)
    {
        try
        {
            return await gameService.GetBySteamToolIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameSlim?>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create a game
    /// </summary>
    /// <param name="request">Required properties to create a game</param>
    /// <param name="gameService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created game</returns>
    [Authorize(PermissionConstants.GameServer.Game.Create)]
    private static async Task<IResult<Guid>> Create(GameCreateRequest request, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.CreateAsync(request, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Update a game's properties
    /// </summary>
    /// <param name="request"></param>
    /// <param name="gameService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Game.Update)]
    private static async Task<IResult> Update(GameUpdateRequest request, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.UpdateAsync(request, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Delete a game
    /// </summary>
    /// <param name="id">Game id</param>
    /// <param name="gameService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Game.Delete)]
    private static async Task<IResult> Delete(Guid id, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.DeleteAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Search a game by it's properties
    /// </summary>
    /// <param name="searchText">Text to search with</param>
    /// <param name="gameService"></param>
    /// <returns>List of games</returns>
    /// <remarks>Searches by: FriendlyName, SteamName, SteamGameId, SteamToolId, Description</remarks>
    [Authorize(PermissionConstants.GameServer.Game.Search)]
    private static async Task<IResult<IEnumerable<GameSlim>>> Search(string searchText, IGameService gameService)
    {
        try
        {
            return await gameService.SearchAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameSlim>>.FailAsync(ex.Message);
        }
    }
}