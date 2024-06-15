using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.Mod;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

public static class ModEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsMod(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetAll, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetByHash, GetByHash).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetByFriendlyName, GetByFriendlyName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetByGameId, GetByGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetBySteamGameId, GetBySteamGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetBySteamId, GetBySteamId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetBySteamToolId, GetBySteamToolId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Mod.Create, Create).ApiVersionOne();
        app.MapPatch(ApiRouteConstants.GameServer.Mod.Update, Update).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Mod.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.Search, Search).ApiVersionOne();
    }
    
    /// <summary>
    /// Get all mods with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of mods</returns>
    [Authorize(PermissionConstants.GameServer.Mod.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<ModSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            if (pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize) pageSize = 500;

            var result = await gameServerService.GetAllModsPaginatedAsync(pageNumber, pageSize);
            if (!result.Succeeded)
                return await Result<IEnumerable<ModSlim>>.FailAsync(result.Messages);
            
            var totalCountRequest = await gameServerService.GetModCountAsync();
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Mod.GetAll,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Mod.GetAll,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<ModSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<ModSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get total mod count
    /// </summary>
    /// <param name="gameServerService"></param>
    /// <returns>Count of total mods</returns>
    [Authorize(PermissionConstants.GameServer.Mod.GetCount)]
    private static async Task<IResult<int>> GetCount(IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetModCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a mod by id
    /// </summary>
    /// <param name="id">Mod id</param>
    /// <param name="gameServerService"></param>
    /// <returns>Mod object</returns>
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
    private static async Task<IResult<ModSlim>> GetById([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetModByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<ModSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a mod by it's integrity hash
    /// </summary>
    /// <param name="hash">Mod hash</param>
    /// <param name="gameServerService"></param>
    /// <returns>Mod object</returns>
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
    private static async Task<IResult<ModSlim>> GetByHash([FromQuery]string hash, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetModByCurrentHashAsync(hash);
        }
        catch (Exception ex)
        {
            return await Result<ModSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get mods by their friendly name
    /// </summary>
    /// <param name="friendlyName">Mod friendly name</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of mods</returns>
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
    private static async Task<IResult<IEnumerable<ModSlim>>> GetByFriendlyName([FromQuery]string friendlyName, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetModsByFriendlyNameAsync(friendlyName);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<ModSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get mods by game id
    /// </summary>
    /// <param name="id">Game id</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of mods</returns>
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
    private static async Task<IResult<IEnumerable<ModSlim>>> GetByGameId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetModsByGameIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<ModSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get mods by steam game id
    /// </summary>
    /// <param name="id">Steam Game Id</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of mods</returns>
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
    private static async Task<IResult<IEnumerable<ModSlim>>> GetBySteamGameId([FromQuery]int id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetModsBySteamGameIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<ModSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get mod by steam id
    /// </summary>
    /// <param name="id">Mod steam id</param>
    /// <param name="gameServerService"></param>
    /// <returns>Mod object</returns>
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
    private static async Task<IResult<ModSlim>> GetBySteamId([FromQuery]string id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetModBySteamIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<ModSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get mods by steam tool id
    /// </summary>
    /// <param name="id">Steam tool id</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of mods</returns>
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
    private static async Task<IResult<IEnumerable<ModSlim>>> GetBySteamToolId([FromQuery]int id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetModsBySteamToolIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<ModSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create a mod
    /// </summary>
    /// <param name="request">Required properties to create a mod</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created mod</returns>
    [Authorize(PermissionConstants.GameServer.Mod.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]ModCreate request, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.CreateModAsync(request, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Update a mod's properties
    /// </summary>
    /// <param name="updateObject">Required properties to update a mod</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Mod.Update)]
    private static async Task<IResult> Update([FromBody]ModUpdate updateObject, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.UpdateModAsync(updateObject, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Delete a mod
    /// </summary>
    /// <param name="id">Mod id</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Mod.Delete)]
    private static async Task<IResult> Delete([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.DeleteModAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Search mods by properties
    /// </summary>
    /// <param name="searchText">Text to search</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of matching mods</returns>
    /// <remarks>Searches by: GameId, SteamGameId, SteamToolId, SteamId, FriendlyName</remarks>
    [Authorize(PermissionConstants.GameServer.Mod.Search)]
    private static async Task<IResult<IEnumerable<ModSlim>>> Search([FromQuery]string searchText, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.SearchModsAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<ModSlim>>.FailAsync(ex.Message);
        }
    }
}