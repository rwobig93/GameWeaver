﻿using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.Developers;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
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
    private static async Task<IResult<IEnumerable<DeveloperSlim>>> GetAllPaginated(int pageNumber, int pageSize, IGameService gameService,
        IOptions<AppConfiguration> appConfig)
    {
        try
        {
            if (pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize) pageSize = 500;

            var result = await gameService.GetAllDevelopersPaginatedAsync(pageNumber, pageSize);
            if (!result.Succeeded)
                return await Result<IEnumerable<DeveloperSlim>>.FailAsync(result.Messages);
            
            var totalCountRequest = await gameService.GetDevelopersCountAsync();
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Developer.GetAll,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Developer.GetAll,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<DeveloperSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
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
    private static async Task<IResult<DeveloperSlim?>> GetById(Guid id, IGameService gameService)
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
    private static async Task<IResult<DeveloperSlim?>> GetByName(string name, IGameService gameService)
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
    private static async Task<IResult<IEnumerable<DeveloperSlim>>> GetByGameId(Guid id, IGameService gameService)
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
    private static async Task<IResult<Guid>> Create(DeveloperCreate createObject, IGameService gameService, ICurrentUserService currentUserService)
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
    private static async Task<IResult> Delete(Guid id, IGameService gameService, ICurrentUserService currentUserService)
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
    /// <param name="gameService"></param>
    /// <returns>List of developers</returns>
    [Authorize(PermissionConstants.GameServer.Developer.Search)]
    private static async Task<IResult<IEnumerable<DeveloperSlim>>> Search(string searchText, IGameService gameService)
    {
        try
        {
            return await gameService.SearchDevelopersAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(ex.Message);
        }
    }
}