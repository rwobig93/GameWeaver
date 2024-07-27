using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameServer;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

public static class ConfigItemEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsConfigItem(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.GameServer.ConfigItem.GetAll, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.ConfigItem.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.ConfigItem.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.ConfigItem.GetByLocalResource, GetByLocalResourceId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.ConfigItem.Create, Create).ApiVersionOne();
        app.MapPatch(ApiRouteConstants.GameServer.ConfigItem.Update, Update).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.ConfigItem.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.ConfigItem.Search, Search).ApiVersionOne();
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
    private static async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;

            var result = await gameServerService.GetAllConfigurationItemsPaginatedAsync(pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<ConfigurationItemSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;

            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.ConfigItem.GetAll, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.ConfigItem.GetAll, pageNumber, pageSize, result.TotalCount);
            return result;
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
    private static async Task<IResult<int>> GetCount(IGameServerService gameServerService)
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
    private static async Task<IResult<ConfigurationItemSlim>> GetById([FromQuery]Guid id, IGameServerService gameServerService)
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
    private static async Task<IResult<IEnumerable<ConfigurationItemSlim>>> GetByLocalResourceId([FromQuery]Guid id, IGameServerService gameServerService)
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
    private static async Task<IResult<Guid>> Create([FromBody]ConfigurationItemCreate createObject, IGameServerService gameServerService,
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
    private static async Task<IResult> Update([FromBody]ConfigurationItemUpdate updateObject, IGameServerService gameServerService,
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
    private static async Task<IResult> Delete([FromQuery]Guid id, IGameServerService gameServerService, ICurrentUserService currentUserService)
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
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameServerService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of matching configuration items</returns>
    /// <remarks>Searches by: ID, GameProfileId, Path, Category, Key, Value</remarks>
    [Authorize(PermissionConstants.GameServer.ConfigItem.Search)]
    private static async Task<IResult<IEnumerable<ConfigurationItemSlim>>> Search([FromQuery]string searchText, [FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameServerService gameServerService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;

            var result = await gameServerService.SearchConfigurationItemsPaginatedAsync(searchText, pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<ConfigurationItemSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;

            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.ConfigItem.Search, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.ConfigItem.Search, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<ConfigurationItemSlim>>.FailAsync(ex.Message);
        }
    }
}