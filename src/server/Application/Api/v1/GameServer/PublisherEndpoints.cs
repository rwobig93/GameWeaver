using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.Publishers;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Api.v1.GameServer;

public static class PublisherEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsPublisher(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.GameServer.Publisher.GetAll, GetAllPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Publisher.GetCount, GetCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Publisher.GetById, GetById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Publisher.GetByName, GetByName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Publisher.GetByGameId, GetByGameId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Publisher.Create, Create).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Publisher.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Publisher.Search, Search).ApiVersionOne();
    }
    
    /// <summary>
    /// Get all publishers with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of publishers</returns>
    [Authorize(PermissionConstants.GameServer.Publisher.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<PublisherSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize, IGameService gameService,
        IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await gameService.GetAllPublishersPaginatedAsync(pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<PublisherSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Publisher.GetAll, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Publisher.GetAll, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get total count of publishers
    /// </summary>
    /// <param name="gameService"></param>
    /// <returns>Count of total publishers</returns>
    [Authorize(PermissionConstants.GameServer.Publisher.GetCount)]
    private static async Task<IResult<int>> GetCount(IGameService gameService)
    {
        try
        {
            return await gameService.GetPublishersCountAsync();
        }
        catch (Exception ex)
        {
            return await Result<int>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a publisher by id
    /// </summary>
    /// <param name="id">Publisher id</param>
    /// <param name="gameService"></param>
    /// <returns>Publisher object</returns>
    [Authorize(PermissionConstants.GameServer.Publisher.Get)]
    private static async Task<IResult<PublisherSlim?>> GetById([FromQuery]Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.GetPublisherByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<PublisherSlim?>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a publisher by name
    /// </summary>
    /// <param name="name">Publisher name</param>
    /// <param name="gameService"></param>
    /// <returns>Publisher object</returns>
    [Authorize(PermissionConstants.GameServer.Publisher.Get)]
    private static async Task<IResult<PublisherSlim?>> GetByName([FromQuery]string name, IGameService gameService)
    {
        try
        {
            return await gameService.GetPublisherByNameAsync(name);
        }
        catch (Exception ex)
        {
            return await Result<PublisherSlim?>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get publishers by game id
    /// </summary>
    /// <param name="id">Game id</param>
    /// <param name="gameService"></param>
    /// <returns>List of publishers</returns>
    [Authorize(PermissionConstants.GameServer.Publisher.Get)]
    private static async Task<IResult<IEnumerable<PublisherSlim>>> GetByGameId([FromQuery]Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.GetPublishersByGameIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Create a publisher
    /// </summary>
    /// <param name="createObject">Required properties to create a publisher</param>
    /// <param name="gameService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Id of the created publisher</returns>
    [Authorize(PermissionConstants.GameServer.Publisher.Create)]
    private static async Task<IResult<Guid>> Create([FromBody]PublisherCreate createObject, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.CreatePublisherAsync(createObject, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Delete a publisher
    /// </summary>
    /// <param name="id">Publisher id</param>
    /// <param name="gameService"></param>
    /// <param name="currentUserService"></param>
    /// <returns>Success or failure with context messages</returns>
    [Authorize(PermissionConstants.GameServer.Publisher.Delete)]
    private static async Task<IResult> Delete([FromQuery]Guid id, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.DeletePublisherAsync(id, currentUserId);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Search for publishers by properties
    /// </summary>
    /// <param name="searchText">Text to search for</param>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of publishers</returns>
    /// <remarks>Searches by: ID, Name</remarks>
    [Authorize(PermissionConstants.GameServer.Publisher.Search)]
    private static async Task<IResult<IEnumerable<PublisherSlim>>> Search([FromQuery]string searchText, [FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameService gameService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;
            
            var result = await gameService.SearchPublishersPaginatedAsync(searchText, pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<PublisherSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;
            
            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Publisher.Search, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Publisher.Search, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(ex.Message);
        }
    }
}