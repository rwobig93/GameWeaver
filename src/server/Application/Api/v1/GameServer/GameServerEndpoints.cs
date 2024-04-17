using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
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
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetAllPaginated, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetByServerName, GetByServerName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetByGameId, GetByGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetByGameProfileId, GetByGameProfileId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetByHostId, GetByHostId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetByOwnerId, GetByOwnerId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.Create, Create).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.Update, Update).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Gameserver.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.Search, Search).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetAllConfigurationItemsPaginated, GetAllConfigurationItemsPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetConfigurationItemsCount, GetConfigurationItemsCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetConfigurationItemById, GetConfigurationItemById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetConfigurationItemsByGameProfileId, GetConfigurationItemsByGameProfileId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.CreateConfigurationItem, CreateConfigurationItem).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.UpdateConfigurationItem, UpdateConfigurationItem).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Gameserver.DeleteConfigurationItem, DeleteConfigurationItem).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.SearchConfigurationItems, SearchConfigurationItems).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetAllLocalResourcesPaginated, GetAllLocalResourcesPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetLocalResourcesCount, GetLocalResourcesCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetLocalResourceById, GetLocalResourceById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetLocalResourcesByGameProfileId, GetLocalResourcesByGameProfileId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetLocalResourcesByGameServerId, GetLocalResourcesByGameServerId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.CreateLocalResource, CreateLocalResource).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.UpdateLocalResource, UpdateLocalResource).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Gameserver.DeleteLocalResource, DeleteLocalResource).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.SearchLocalResource, SearchLocalResource).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetAllGameProfilesPaginated, GetAllGameProfilesPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetGameProfileCount, GetGameProfileCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetGameProfileById, GetGameProfileById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetGameProfileByFriendlyName, GetGameProfileByFriendlyName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetGameProfilesByGameId, GetGameProfilesByGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetGameProfilesByOwnerId, GetGameProfilesByOwnerId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetGameProfilesByServerProcessName, GetGameProfilesByServerProcessName).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.CreateGameProfile, CreateGameProfile).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.UpdateGameProfile, UpdateGameProfile).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Gameserver.DeleteGameProfile, DeleteGameProfile).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.SearchGameProfiles, SearchGameProfiles).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetAllModsPaginated, GetAllModsPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetModCount, GetModCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetModById, GetModById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetModByCurrentHash, GetModByCurrentHash).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetModsByFriendlyName, GetModsByFriendlyName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetModsByGameId, GetModsByGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetModsBySteamGameId, GetModsBySteamGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetModBySteamId, GetModBySteamId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.GetModsBySteamToolId, GetModsBySteamToolId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.CreateMod, CreateMod).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.UpdateMod, UpdateMod).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Gameserver.DeleteMod, DeleteMod).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.SearchMods, SearchMods).ApiVersionOne();
    }
    
    /// <summary>
    /// Get all game servers with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.Gameserver.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<GameServerSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize, IGameServerService gameServerService, 
        IOptions<AppConfiguration> appConfig)
    {
        try
        {
            if (pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize) pageSize = 500;

            var result = await gameServerService.GetAllPaginatedAsync(pageNumber, pageSize);
            if (!result.Succeeded)
                return await Result<IEnumerable<GameServerSlim>>.FailAsync(result.Messages);
            
            var totalCountRequest = await gameServerService.GetCountAsync();
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Gameserver.GetAllPaginated,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Gameserver.GetAllPaginated,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<GameServerSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
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
    [Authorize(PermissionConstants.Gameserver.GetCount)]
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
    /// <param name="id">Id of the game server</param>
    /// <param name="gameServerService"></param>
    /// <returns>Game server object</returns>
    [Authorize(PermissionConstants.Gameserver.GetById)]
    private static async Task<IResult<GameServerSlim>> GetById([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetByIdAsync(id);
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
    /// <returns>Game server object</returns>
    [Authorize(PermissionConstants.Gameserver.GetByServerName)]
    private static async Task<IResult<GameServerSlim>> GetByServerName([FromQuery]string serverName, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetByServerNameAsync(serverName);
        }
        catch (Exception ex)
        {
            return await Result<GameServerSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get game servers by game id
    /// </summary>
    /// <param name="id">Id of a game</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.Gameserver.GetByGameId)]
    private static async Task<IResult<GameServerSlim>> GetByGameId([FromQuery]int id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetByGameIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameServerSlim>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get game servers by game profile id
    /// </summary>
    /// <param name="id">Game profile id</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.Gameserver.GetByGameProfileId)]
    private static async Task<IResult<GameServerSlim>> GetByGameProfileId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            // TODO: Get by methods for game server service (and repository) needs to return IEnumerable instead of first since they are 1:many not 1:1
            return await gameServerService.GetByGameProfileIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameServerSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get game servers by host id
    /// </summary>
    /// <param name="id">Host id</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.Gameserver.GetByHostId)]
    private static async Task<IResult<GameServerSlim>> GetByHostId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetByHostIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameServerSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get game servers by the owner id
    /// </summary>
    /// <param name="id">Id of the owner</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.Gameserver.GetByOwnerId)]
    private static async Task<IResult<GameServerSlim>> GetByOwnerId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetByOwnerIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameServerSlim>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create a game server
    /// </summary>
    /// <param name="createObject">Required properties to create a game server</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created game server</returns>
    [Authorize(PermissionConstants.Gameserver.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]GameServerCreate createObject, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            createObject.CreatedBy = currentUserId;
            return await gameServerService.CreateAsync(createObject);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Update a game server's properties
    /// </summary>
    /// <param name="updateObject">Required properties to update a game server</param>
    /// <param name="gameServerService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.Gameserver.Update)]
    private static async Task<IResult> Update([FromBody]GameServerUpdate updateObject, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.UpdateAsync(updateObject);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Delete a game server
    /// </summary>
    /// <param name="id">Id of the game server</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.Gameserver.Delete)]
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
    /// <param name="gameServerService"></param>
    /// <returns>List of matching game servers</returns>
    /// <remarks>Searches by: OwnerId, HostId, GameId, GameProfileId, PublicIp, PrivateIp, ExternalHostname, ServerName</remarks>
    [Authorize(PermissionConstants.Gameserver.Search)]
    private static async Task<IResult<IEnumerable<GameServerSlim>>> Search([FromQuery]string searchText, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.SearchAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get all configuration items with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of configuration items</returns>
    [Authorize(PermissionConstants.Gameserver.GetAllConfigurationItemsPaginated)]
    private static async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetAllConfigurationItemsPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            if (pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize) pageSize = 500;

            var result = await gameServerService.GetAllConfigurationItemsPaginatedAsync(pageNumber, pageSize);
            if (!result.Succeeded)
                return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(result.Messages);
            
            var totalCountRequest = await gameServerService.GetConfigurationItemsCountAsync();
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Gameserver.GetAllConfigurationItemsPaginated,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Gameserver.GetAllConfigurationItemsPaginated,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<ConfigurationItemSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get total configuration items count
    /// </summary>
    /// <param name="gameServerService"></param>
    /// <returns>Count of configuration items</returns>
    [Authorize(PermissionConstants.Gameserver.GetConfigurationItemsCount)]
    private static async Task<IResult<int>> GetConfigurationItemsCount(IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetConfigurationItemsCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a configuration item by id
    /// </summary>
    /// <param name="id">Configuration item id</param>
    /// <param name="gameServerService"></param>
    /// <returns>Configuration item object</returns>
    [Authorize(PermissionConstants.Gameserver.GetConfigurationItemById)]
    private static async Task<IResult<ConfigurationItemSlim>> GetConfigurationItemById([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetConfigurationItemByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<ConfigurationItemSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get configuration times by game profile id
    /// </summary>
    /// <param name="id">Game profile id</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of configuration items</returns>
    [Authorize(PermissionConstants.Gameserver.GetConfigurationItemsByGameProfileId)]
    private static async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetConfigurationItemsByGameProfileId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetConfigurationItemsByGameProfileIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Create a configuration item
    /// </summary>
    /// <param name="createObject">Required properties to create a configuration item</param>
    /// <param name="gameServerService"></param>
    /// <returns>Id of the created configuration item</returns>
    [Authorize(PermissionConstants.Gameserver.CreateConfigurationItem)]
    private static async Task<IResult<Guid>> CreateConfigurationItem([FromBody]ConfigurationItemCreate createObject, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.CreateConfigurationItemAsync(createObject);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Update a configuration items properties
    /// </summary>
    /// <param name="updateObject">Required properties to update a configuration item</param>
    /// <param name="gameServerService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.Gameserver.UpdateConfigurationItem)]
    private static async Task<IResult> UpdateConfigurationItem([FromBody]ConfigurationItemUpdate updateObject, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.UpdateConfigurationItemAsync(updateObject);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Delete a configuration item
    /// </summary>
    /// <param name="id">Configuration item id</param>
    /// <param name="gameServerService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.Gameserver.DeleteConfigurationItem)]
    private static async Task<IResult> DeleteConfigurationItem([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.DeleteConfigurationItemAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Search for configuration items by properties
    /// </summary>
    /// <param name="searchText">Text to search for</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of matching configuration items</returns>
    /// <remarks>Searches by: GameProfileId, Path, Category, Key, Value</remarks>
    [Authorize(PermissionConstants.Gameserver.SearchConfigurationItems)]
    private static async Task<IResult<IEnumerable<ConfigurationItemSlim>>> SearchConfigurationItems([FromQuery]string searchText, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.SearchConfigurationItemsAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get all local resources with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of local resources</returns>
    [Authorize(PermissionConstants.Gameserver.GetAllLocalResourcesPaginated)]
    private static async Task<IResult<IEnumerable<LocalResourceSlim>>> GetAllLocalResourcesPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            if (pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize) pageSize = 500;

            var result = await gameServerService.GetAllLocalResourcesPaginatedAsync(pageNumber, pageSize);
            if (!result.Succeeded)
                return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(result.Messages);
            
            var totalCountRequest = await gameServerService.GetLocalResourcesCountAsync();
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Gameserver.GetAllLocalResourcesPaginated,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Gameserver.GetAllLocalResourcesPaginated,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<LocalResourceSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
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
    [Authorize(PermissionConstants.Gameserver.GetLocalResourcesCount)]
    private static async Task<IResult<int>> GetLocalResourcesCount(IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetLocalResourceById)]
    private static async Task<IResult<LocalResourceSlim>> GetLocalResourceById([FromQuery]Guid id, IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetLocalResourcesByGameProfileId)]
    private static async Task<IResult<IEnumerable<LocalResourceSlim>>> GetLocalResourcesByGameProfileId([FromQuery]Guid id, IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetLocalResourcesByGameServerId)]
    private static async Task<IResult<IEnumerable<LocalResourceSlim>>> GetLocalResourcesByGameServerId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetLocalResourcesByGameServerIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Create a local resource
    /// </summary>
    /// <param name="createObject"></param>
    /// <param name="gameServerService"></param>
    /// <returns>Id of the created local resource</returns>
    [Authorize(PermissionConstants.Gameserver.CreateLocalResource)]
    private static async Task<IResult<Guid>> CreateLocalResource([FromBody]LocalResourceCreate createObject, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.CreateLocalResourceAsync(createObject);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Update a local resources properties
    /// </summary>
    /// <param name="updateObject">Required properties to update a local resource</param>
    /// <param name="gameServerService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.Gameserver.UpdateLocalResource)]
    private static async Task<IResult> UpdateLocalResource([FromBody]LocalResourceUpdate updateObject, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.UpdateLocalResourceAsync(updateObject);
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
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.Gameserver.DeleteLocalResource)]
    private static async Task<IResult> DeleteLocalResource([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.DeleteLocalResourceAsync(id);
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
    /// <param name="gameServerService"></param>
    /// <returns>List of matching local resources</returns>
    /// <remarks>Searches by: GameProfileId, GameServerId, Name, Path, Extension, Args</remarks>
    [Authorize(PermissionConstants.Gameserver.SearchLocalResource)]
    private static async Task<IResult<IEnumerable<LocalResourceSlim>>> SearchLocalResource([FromQuery]string searchText, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.SearchLocalResourceAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<LocalResourceSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get all game profiles with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of game profiles</returns>
    [Authorize(PermissionConstants.Gameserver.GetAllGameProfilesPaginated)]
    private static async Task<IResult<IEnumerable<GameProfileSlim>>> GetAllGameProfilesPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            if (pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize) pageSize = 500;

            var result = await gameServerService.GetAllGameProfilesPaginatedAsync(pageNumber, pageSize);
            if (!result.Succeeded)
                return await Result<IEnumerable<GameProfileSlim>>.FailAsync(result.Messages);
            
            var totalCountRequest = await gameServerService.GetGameProfileCountAsync();
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Gameserver.GetAllGameProfilesPaginated,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Gameserver.GetAllGameProfilesPaginated,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<GameProfileSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
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
    [Authorize(PermissionConstants.Gameserver.GetGameProfileCount)]
    private static async Task<IResult<int>> GetGameProfileCount(IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetGameProfileById)]
    private static async Task<IResult<GameProfileSlim>> GetGameProfileById([FromQuery]Guid id, IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetGameProfileByFriendlyName)]
    private static async Task<IResult<GameProfileSlim>> GetGameProfileByFriendlyName([FromQuery]string friendlyName, IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetGameProfilesByGameId)]
    private static async Task<IResult<IEnumerable<GameProfileSlim>>> GetGameProfilesByGameId([FromQuery]int id, IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetGameProfilesByOwnerId)]
    private static async Task<IResult<IEnumerable<GameProfileSlim>>> GetGameProfilesByOwnerId([FromQuery]Guid id, IGameServerService gameServerService)
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
    /// Get game profiles by server process name
    /// </summary>
    /// <param name="serverProcessName">Executable / Binary game server process name</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of game profiles</returns>
    [Authorize(PermissionConstants.Gameserver.GetGameProfilesByServerProcessName)]
    private static async Task<IResult<IEnumerable<GameProfileSlim>>> GetGameProfilesByServerProcessName([FromQuery]string serverProcessName, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetGameProfilesByServerProcessNameAsync(serverProcessName);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Create a game profile
    /// </summary>
    /// <param name="createObject">Required properties to create a game profile</param>
    /// <param name="gameServerService"></param>
    /// <returns>Id of the created game profile</returns>
    [Authorize(PermissionConstants.Gameserver.CreateGameProfile)]
    private static async Task<IResult<Guid>> CreateGameProfile([FromBody]GameProfileCreate createObject, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.CreateGameProfileAsync(createObject);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Update a game profiles properties
    /// </summary>
    /// <param name="updateObject">Required properties to update a game profile</param>
    /// <param name="gameServerService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.Gameserver.UpdateGameProfile)]
    private static async Task<IResult> UpdateGameProfile([FromBody]GameProfileUpdate updateObject, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.UpdateGameProfileAsync(updateObject);
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
    [Authorize(PermissionConstants.Gameserver.DeleteGameProfile)]
    private static async Task<IResult> DeleteGameProfile([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
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
    /// <param name="gameServerService"></param>
    /// <returns>List of matching game profiles</returns>
    /// <remarks>Searches by: FriendlyName, OwnerId, GameId, ServerProcessName</remarks>
    [Authorize(PermissionConstants.Gameserver.SearchGameProfiles)]
    private static async Task<IResult<IEnumerable<GameProfileSlim>>> SearchGameProfiles([FromQuery]string searchText, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.SearchGameProfilesAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameProfileSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get all mods with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of mods</returns>
    [Authorize(PermissionConstants.Gameserver.GetAllModsPaginated)]
    private static async Task<IResult<IEnumerable<ModSlim>>> GetAllModsPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            if (pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize) pageSize = 500;

            var result = await gameServerService.GetAllModsPaginatedAsync(pageNumber, pageSize);
            if (!result.Succeeded)
                return await Result<IEnumerable<ModSlim>>.FailAsync(result.Messages);
            
            var totalCountRequest = await gameServerService.GetModCountAsync();
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Gameserver.GetAllModsPaginated,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Gameserver.GetAllModsPaginated,
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
    [Authorize(PermissionConstants.Gameserver.GetModCount)]
    private static async Task<IResult<int>> GetModCount(IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetModById)]
    private static async Task<IResult<ModSlim>> GetModById([FromQuery]Guid id, IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetModByCurrentHash)]
    private static async Task<IResult<ModSlim>> GetModByCurrentHash([FromQuery]string hash, IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetModsByFriendlyName)]
    private static async Task<IResult<IEnumerable<ModSlim>>> GetModsByFriendlyName([FromQuery]string friendlyName, IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetModsByGameId)]
    private static async Task<IResult<IEnumerable<ModSlim>>> GetModsByGameId([FromQuery]Guid id, IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetModsBySteamGameId)]
    private static async Task<IResult<IEnumerable<ModSlim>>> GetModsBySteamGameId([FromQuery]int id, IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetModBySteamId)]
    private static async Task<IResult<ModSlim>> GetModBySteamId([FromQuery]string id, IGameServerService gameServerService)
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
    [Authorize(PermissionConstants.Gameserver.GetModsBySteamToolId)]
    private static async Task<IResult<IEnumerable<ModSlim>>> GetModsBySteamToolId([FromQuery]int id, IGameServerService gameServerService)
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
    /// <param name="createObject">Required properties to create a mod</param>
    /// <param name="gameServerService"></param>
    /// <returns>Id of the created mod</returns>
    [Authorize(PermissionConstants.Gameserver.CreateMod)]
    private static async Task<IResult<Guid>> CreateMod([FromBody]ModCreate createObject, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.CreateModAsync(createObject);
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
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.Gameserver.UpdateMod)]
    private static async Task<IResult> UpdateMod([FromBody]ModUpdate updateObject, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.UpdateModAsync(updateObject);
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
    [Authorize(PermissionConstants.Gameserver.DeleteMod)]
    private static async Task<IResult> DeleteMod([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
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
    [Authorize(PermissionConstants.Gameserver.SearchMods)]
    private static async Task<IResult<IEnumerable<ModSlim>>> SearchMods([FromQuery]string searchText, IGameServerService gameServerService)
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