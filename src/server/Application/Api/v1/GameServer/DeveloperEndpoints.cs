using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.Developers;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

public static class DeveloperEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsDeveloper(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.GameServer.Developer.GetAll, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Developer.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Developer.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Developer.GetByName, GetByName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Developer.GetByGameId, GetByGameId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Developer.Create, Create).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Developer.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Developer.Search, Search).ApiVersionOne();
    }
    
    /// <summary>
    /// Get all developers with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of developers</returns>
    [Authorize(PermissionConstants.GameServer.Developer.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<DeveloperSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize, IGameService gameService,
        IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await gameService.GetAllDevelopersPaginatedAsync(pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<DeveloperSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Developer.GetAll, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Developer.GetAll, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get count of total developers
    /// </summary>
    /// <param name="gameService"></param>
    /// <returns>Count of total developers</returns>
    [Authorize(PermissionConstants.GameServer.Developer.GetCount)]
    private static async Task<IResult<int>> GetCount(IGameService gameService)
    {
        try
        {
            return await gameService.GetDevelopersCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a developers by id
    /// </summary>
    /// <param name="id">Developer id</param>
    /// <param name="gameService"></param>
    /// <returns>Developer object</returns>
    [Authorize(PermissionConstants.GameServer.Developer.Get)]
    private static async Task<IResult<DeveloperSlim?>> GetById([FromQuery]Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.GetDeveloperByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<DeveloperSlim?>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a developer by name
    /// </summary>
    /// <param name="name">Developer name</param>
    /// <param name="gameService"></param>
    /// <returns>Developer object</returns>
    [Authorize(PermissionConstants.GameServer.Developer.Get)]
    private static async Task<IResult<DeveloperSlim?>> GetByName([FromQuery]string name, IGameService gameService)
    {
        try
        {
            return await gameService.GetDeveloperByNameAsync(name);
        }
        catch (Exception ex)
        {
            return await Result<DeveloperSlim?>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get developers by game id
    /// </summary>
    /// <param name="id">Game id</param>
    /// <param name="gameService"></param>
    /// <returns>List of developers</returns>
    [Authorize(PermissionConstants.GameServer.Developer.Get)]
    private static async Task<IResult<IEnumerable<DeveloperSlim>>> GetByGameId([FromQuery]Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.GetDevelopersByGameIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create a developer
    /// </summary>
    /// <param name="createObject">Required properties to create a developer</param>
    /// <param name="gameService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created developer</returns>
    [Authorize(PermissionConstants.GameServer.Developer.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]DeveloperCreate createObject, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.CreateDeveloperAsync(createObject, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Delete a developer
    /// </summary>
    /// <param name="id">Developer id</param>
    /// <param name="gameService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Developer.Delete)]
    private static async Task<IResult> Delete([FromQuery]Guid id, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.DeleteDeveloperAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Search developers by properties
    /// </summary>
    /// <param name="searchText">Text to search for</param>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of developers</returns>
    /// <remarks>Searches by: ID, Name</remarks>
    [Authorize(PermissionConstants.GameServer.Developer.Search)]
    private static async Task<IResult<IEnumerable<DeveloperSlim>>> Search([FromQuery]string searchText, [FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameService gameService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await gameService.SearchDevelopersPaginatedAsync(searchText, pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<DeveloperSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Developer.Search, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Developer.Search, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(ex.Message);
        }
    }
}