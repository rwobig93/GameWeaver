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
        app.MapGet(PermissionConstants.Gameserver.GetAllPaginated, GetAllPaginated).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetCount, GetCount).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetById, GetById).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetByServerName, GetByServerName).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetByGameId, GetByGameId).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetByGameProfileId, GetByGameProfileId).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetByHostId, GetByHostId).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetByOwnerId, GetByOwnerId).ApiVersionOne();
        app.MapPost(PermissionConstants.Gameserver.Create, Create).ApiVersionOne();
        app.MapPost(PermissionConstants.Gameserver.Update, Update).ApiVersionOne();
        app.MapDelete(PermissionConstants.Gameserver.Delete, Delete).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.Search, Search).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetAllConfigurationItemsPaginated, GetAllConfigurationItemsPaginated).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetConfigurationItemsCount, GetConfigurationItemsCount).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetConfigurationItemById, GetConfigurationItemById).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetConfigurationItemsByGameProfileId, GetConfigurationItemsByGameProfileId).ApiVersionOne();
        app.MapPost(PermissionConstants.Gameserver.CreateConfigurationItem, CreateConfigurationItem).ApiVersionOne();
        app.MapPost(PermissionConstants.Gameserver.UpdateConfigurationItem, UpdateConfigurationItem).ApiVersionOne();
        app.MapDelete(PermissionConstants.Gameserver.DeleteConfigurationItem, DeleteConfigurationItem).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.SearchConfigurationItems, SearchConfigurationItems).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetAllLocalResourcesPaginated, GetAllLocalResourcesPaginated).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetLocalResourcesCount, GetLocalResourcesCount).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetLocalResourceById, GetLocalResourceById).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetLocalResourcesByGameProfileId, GetLocalResourcesByGameProfileId).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetLocalResourcesByGameServerId, GetLocalResourcesByGameServerId).ApiVersionOne();
        app.MapPost(PermissionConstants.Gameserver.CreateLocalResource, CreateLocalResource).ApiVersionOne();
        app.MapPost(PermissionConstants.Gameserver.UpdateLocalResource, UpdateLocalResource).ApiVersionOne();
        app.MapDelete(PermissionConstants.Gameserver.DeleteLocalResource, DeleteLocalResource).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.SearchLocalResource, SearchLocalResource).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetAllGameProfilesPaginated, GetAllGameProfilesPaginated).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetGameProfileCount, GetGameProfileCount).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetGameProfileById, GetGameProfileById).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetGameProfileByFriendlyName, GetGameProfileByFriendlyName).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetGameProfilesByGameId, GetGameProfilesByGameId).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetGameProfilesByOwnerId, GetGameProfilesByOwnerId).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetGameProfilesByServerProcessName, GetGameProfilesByServerProcessName).ApiVersionOne();
        app.MapPost(PermissionConstants.Gameserver.CreateGameProfile, CreateGameProfile).ApiVersionOne();
        app.MapPost(PermissionConstants.Gameserver.UpdateGameProfile, UpdateGameProfile).ApiVersionOne();
        app.MapDelete(PermissionConstants.Gameserver.DeleteGameProfile, DeleteGameProfile).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.SearchGameProfiles, SearchGameProfiles).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetAllModsPaginated, GetAllModsPaginated).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetModCount, GetModCount).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetModById, GetModById).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetModByCurrentHash, GetModByCurrentHash).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetModsByFriendlyName, GetModsByFriendlyName).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetModsByGameId, GetModsByGameId).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetModsBySteamGameId, GetModsBySteamGameId).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetModBySteamId, GetModBySteamId).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.GetModsBySteamToolId, GetModsBySteamToolId).ApiVersionOne();
        app.MapPost(PermissionConstants.Gameserver.CreateMod, CreateMod).ApiVersionOne();
        app.MapPost(PermissionConstants.Gameserver.UpdateMod, UpdateMod).ApiVersionOne();
        app.MapDelete(PermissionConstants.Gameserver.DeleteMod, DeleteMod).ApiVersionOne();
        app.MapGet(PermissionConstants.Gameserver.SearchMods, SearchMods).ApiVersionOne();
    }
    

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


    [Authorize(PermissionConstants.Gameserver.GetByGameProfileId)]
    private static async Task<IResult<GameServerSlim>> GetByGameProfileId([FromQuery]Guid id, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetByGameProfileIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameServerSlim>.FailAsync(ex.Message);
        }
    }


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


    [Authorize(PermissionConstants.Gameserver.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]GameServerCreate createObject, IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.CreateAsync(createObject);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }


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