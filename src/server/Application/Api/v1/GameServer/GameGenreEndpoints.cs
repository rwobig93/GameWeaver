using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.GameGenre;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

public static class GameGenreEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsGameGenre(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.GetAll, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.GetByName, GetByName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.GetByGameId, GetByGameId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.GameGenre.Create, Create).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.GameGenre.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.Search, Search).ApiVersionOne();
    }
    
    /// <summary>
    /// Get all game genres with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of game genres</returns>
    [Authorize(PermissionConstants.GameServer.GameGenre.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<GameGenreSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize, IGameService gameService,
        IOptions<AppConfiguration> appConfig)
    {
        try
        {
            if (pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize) pageSize = 500;

            var result = await gameService.GetAllGameGenresPaginatedAsync(pageNumber, pageSize);
            if (!result.Succeeded)
                return await Result<IEnumerable<GameGenreSlim>>.FailAsync(result.Messages);
            
            var totalCountRequest = await gameService.GetGameGenresCountAsync();
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.GameGenre.GetAll,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.GameGenre.GetAll,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<GameGenreSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameGenreSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get total count of game genres
    /// </summary>
    /// <param name="gameService"></param>
    /// <returns>Count of total game genres</returns>
    [Authorize(PermissionConstants.GameServer.GameGenre.GetCount)]
    private static async Task<IResult<int>> GetCount(IGameService gameService)
    {
        try
        {
            return await gameService.GetGameGenresCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game genre by id
    /// </summary>
    /// <param name="id">Game genre id</param>
    /// <param name="gameService"></param>
    /// <returns>Game genre object</returns>
    [Authorize(PermissionConstants.GameServer.GameGenre.Get)]
    private static async Task<IResult<GameGenreSlim?>> GetById([FromQuery]Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.GetGameGenreByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameGenreSlim?>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game genre by name
    /// </summary>
    /// <param name="name">Game genre name</param>
    /// <param name="gameService"></param>
    /// <returns>Game genre object</returns>
    [Authorize(PermissionConstants.GameServer.GameGenre.Get)]
    private static async Task<IResult<GameGenreSlim?>> GetByName([FromQuery]string name, IGameService gameService)
    {
        try
        {
            return await gameService.GetGameGenreByNameAsync(name);
        }
        catch (Exception ex)
        {
            return await Result<GameGenreSlim?>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get game genres by game id
    /// </summary>
    /// <param name="id">Game id</param>
    /// <param name="gameService"></param>
    /// <returns>List of game genres</returns>
    [Authorize(PermissionConstants.GameServer.GameGenre.Get)]
    private static async Task<IResult<IEnumerable<GameGenreSlim>>> GetByGameId([FromQuery]Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.GetGameGenresByGameIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameGenreSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create a game genre
    /// </summary>
    /// <param name="createObject">Requires properties to create a game genre</param>
    /// <param name="gameService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created game genre</returns>
    [Authorize(PermissionConstants.GameServer.GameGenre.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]GameGenreCreate createObject, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.CreateGameGenreAsync(createObject, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Delete a game genre
    /// </summary>
    /// <param name="id">Game genre id</param>
    /// <param name="gameService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.GameGenre.Delete)]
    private static async Task<IResult> Delete([FromQuery]Guid id, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.DeleteGameGenreAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Search game genres by properties
    /// </summary>
    /// <param name="searchText">Text to search for</param>
    /// <param name="gameService"></param>
    /// <returns>List of game genres</returns>
    /// <remarks>Searches by: Name, Description</remarks>
    [Authorize(PermissionConstants.GameServer.GameGenre.Search)]
    private static async Task<IResult<IEnumerable<GameGenreSlim>>> Search([FromQuery]string searchText, IGameService gameService)
    {
        try
        {
            return await gameService.SearchGameGenresAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameGenreSlim>>.FailAsync(ex.Message);
        }
    }
}