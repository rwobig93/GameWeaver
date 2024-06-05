using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.GameServer.Mod;
using Application.Requests.GameServer.GameProfile;
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
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.Update, Update).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Gameserver.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Gameserver.Search, Search).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.ConfigItem.GetAll, GetAllConfigurationItemsPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.ConfigItem.GetCount, GetConfigurationItemsCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.ConfigItem.GetById, GetConfigurationItemById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.ConfigItem.GetByLocalResource, GetConfigurationItemsByLocalResourceId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.ConfigItem.Create, CreateConfigurationItem).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.ConfigItem.Update, UpdateConfigurationItem).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.ConfigItem.Delete, DeleteConfigurationItem).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.ConfigItem.Search, SearchConfigurationItems).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.GetAllPaginated, GetAllLocalResourcesPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.GetCount, GetLocalResourcesCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.GetById, GetLocalResourceById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.GetByGameProfileId, GetLocalResourcesByGameProfileId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.GetForGameServerId, GetLocalResourcesByGameServerId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.LocalResource.Create, CreateLocalResource).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.LocalResource.Update, UpdateLocalResource).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.LocalResource.Delete, DeleteLocalResource).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.LocalResource.Search, SearchLocalResource).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetAll, GetAllGameProfilesPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetCount, GetGameProfileCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetById, GetGameProfileById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetByFriendlyName, GetGameProfileByFriendlyName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetByGameId, GetGameProfilesByGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.GetByOwnerId, GetGameProfilesByOwnerId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.GameProfile.Create, CreateGameProfile).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.GameProfile.Update, UpdateGameProfile).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.GameProfile.Delete, DeleteGameProfile).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameProfile.Search, SearchGameProfiles).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetAll, GetAllModsPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetCount, GetModCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetById, GetModById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetByHash, GetModByCurrentHash).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetByFriendlyName, GetModsByFriendlyName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetByGameId, GetModsByGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetBySteamGameId, GetModsBySteamGameId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetBySteamId, GetModBySteamId).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.GetBySteamToolId, GetModsBySteamToolId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Mod.Create, CreateMod).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Mod.Update, UpdateMod).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Mod.Delete, DeleteMod).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Mod.Search, SearchMods).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.StartServer, StartServer).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.StopServer, StopServer).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.RestartServer, RestartServer).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.UpdateLocalResource, UpdateLocalResourceOnGameServer).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Gameserver.UpdateAllLocalResources, UpdateAllLocalResourcesOnGameServer).ApiVersionOne();
    }
    
    /// <summary>
    /// Get all game servers with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.GetAllPaginated)]
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
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Gameserver.GetAll,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Gameserver.GetAll,
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
    /// <param name="id">Id of the game server</param>
    /// <param name="gameServerService"></param>
    /// <returns>Game server object</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
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
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
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
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
    private static async Task<IResult<IEnumerable<GameServerSlim>>> GetByGameId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetByGameIdAsync(id);
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
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
    private static async Task<IResult<IEnumerable<GameServerSlim>>> GetByGameProfileId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            // TODO: Get by methods for game server service (and repository) needs to return IEnumerable instead of first since they are 1:many not 1:1
            return await gameServerService.GetByGameProfileIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get game servers by host id
    /// </summary>
    /// <param name="id">Host id</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
    private static async Task<IResult<IEnumerable<GameServerSlim>>> GetByHostId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetByHostIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get game servers by the owner id
    /// </summary>
    /// <param name="id">Id of the owner</param>
    /// <param name="gameServerService"></param>
    /// <returns>List of game servers</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Get)]
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
    /// <param name="request">Required properties to create a game server</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created game server</returns>
    [Authorize(PermissionConstants.GameServer.Gameserver.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]GameServerCreateRequest request, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.CreateAsync(request, currentUserId);
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
            return await gameServerService.UpdateAsync(request, currentUserId);
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
    /// <param name="gameServerService"></param>
    /// <returns>List of matching game servers</returns>
    /// <remarks>Searches by: OwnerId, HostId, GameId, GameProfileId, PublicIp, PrivateIp, ExternalHostname, ServerName</remarks>
    [Authorize(PermissionConstants.GameServer.Gameserver.Search)]
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
    [Authorize(PermissionConstants.GameServer.ConfigItem.GetAllPaginated)]
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
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.ConfigItem.GetAll,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.ConfigItem.GetAll,
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
    [Authorize(PermissionConstants.GameServer.ConfigItem.GetCount)]
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
    [Authorize(PermissionConstants.GameServer.ConfigItem.Get)]
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
    [Authorize(PermissionConstants.GameServer.ConfigItem.Get)]
    private static async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetConfigurationItemsByLocalResourceId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetConfigurationItemsByLocalResourceIdAsync(id);
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
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created configuration item</returns>
    [Authorize(PermissionConstants.GameServer.ConfigItem.Create)]
    private static async Task<IResult<Guid>> CreateConfigurationItem([FromBody]ConfigurationItemCreate createObject, IGameServerService gameServerService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.CreateConfigurationItemAsync(createObject, currentUserId);
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
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.ConfigItem.Update)]
    private static async Task<IResult> UpdateConfigurationItem([FromBody]ConfigurationItemUpdate updateObject, IGameServerService gameServerService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.UpdateConfigurationItemAsync(updateObject, currentUserId);
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
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.ConfigItem.Delete)]
    private static async Task<IResult> DeleteConfigurationItem([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var userId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.DeleteConfigurationItemAsync(id, userId);
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
    [Authorize(PermissionConstants.GameServer.ConfigItem.Search)]
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
    [Authorize(PermissionConstants.GameServer.LocalResource.GetAllPaginated)]
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
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.LocalResource.GetAllPaginated,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.LocalResource.GetAllPaginated,
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
    [Authorize(PermissionConstants.GameServer.LocalResource.GetCount)]
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
    [Authorize(PermissionConstants.GameServer.LocalResource.Get)]
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
    [Authorize(PermissionConstants.GameServer.LocalResource.Get)]
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
    [Authorize(PermissionConstants.GameServer.LocalResource.GetForGameServerId)]
    private static async Task<IResult<IEnumerable<LocalResourceSlim>>> GetLocalResourcesByGameServerId([FromQuery]Guid id, IGameServerService gameServerService)
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
    /// <param name="createObject"></param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created local resource</returns>
    [Authorize(PermissionConstants.GameServer.LocalResource.Create)]
    private static async Task<IResult<Guid>> CreateLocalResource([FromBody]LocalResourceCreate createObject, IGameServerService gameServerService,
        ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.CreateLocalResourceAsync(createObject, currentUserId);
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
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.LocalResource.Update)]
    private static async Task<IResult> UpdateLocalResource([FromBody]LocalResourceUpdate updateObject, IGameServerService gameServerService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameServerService.UpdateLocalResourceAsync(updateObject, currentUserId);
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
    private static async Task<IResult> DeleteLocalResource([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
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
    /// <param name="gameServerService"></param>
    /// <returns>List of matching local resources</returns>
    /// <remarks>Searches by: GameProfileId, GameServerId, Name, Path, Extension, Args</remarks>
    [Authorize(PermissionConstants.GameServer.LocalResource.Search)]
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
    [Authorize(PermissionConstants.GameServer.GameProfile.GetAllPaginated)]
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
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.GameProfile.GetAll,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.GameProfile.GetAll,
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
    [Authorize(PermissionConstants.GameServer.GameProfile.GetCount)]
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
    [Authorize(PermissionConstants.GameServer.GameProfile.Get)]
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
    [Authorize(PermissionConstants.GameServer.GameProfile.Get)]
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
    [Authorize(PermissionConstants.GameServer.GameProfile.Get)]
    private static async Task<IResult<IEnumerable<GameProfileSlim>>> GetGameProfilesByGameId([FromQuery]Guid id, IGameServerService gameServerService)
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
    /// Create a game profile
    /// </summary>
    /// <param name="request">Required properties to create a game profile</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created game profile</returns>
    [Authorize(PermissionConstants.GameServer.GameProfile.Create)]
    private static async Task<IResult<Guid>> CreateGameProfile([FromBody]GameProfileCreateRequest request, IGameServerService gameServerService,
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
    private static async Task<IResult> UpdateGameProfile([FromBody]GameProfileUpdateRequest request, IGameServerService gameServerService, ICurrentUserService currentUserService)
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
    [Authorize(PermissionConstants.GameServer.GameProfile.Search)]
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
    [Authorize(PermissionConstants.GameServer.Mod.GetAllPaginated)]
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
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
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
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
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
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
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
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
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
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
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
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
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
    [Authorize(PermissionConstants.GameServer.Mod.Get)]
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
    /// <param name="request">Required properties to create a mod</param>
    /// <param name="gameServerService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created mod</returns>
    [Authorize(PermissionConstants.GameServer.Mod.Create)]
    private static async Task<IResult<Guid>> CreateMod([FromBody]ModCreate request, IGameServerService gameServerService, ICurrentUserService currentUserService)
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
    private static async Task<IResult> UpdateMod([FromBody]ModUpdate updateObject, IGameServerService gameServerService, ICurrentUserService currentUserService)
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
    [Authorize(PermissionConstants.GameServer.Mod.Search)]
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
    private static async Task<IResult> UpdateLocalResourceOnGameServer([FromQuery]Guid serverId, [FromQuery]Guid resourceId, IGameServerService gameServerService,
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
    private static async Task<IResult> UpdateAllLocalResourcesOnGameServer([FromQuery]Guid serverId, IGameServerService gameServerService, ICurrentUserService currentUserService)
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