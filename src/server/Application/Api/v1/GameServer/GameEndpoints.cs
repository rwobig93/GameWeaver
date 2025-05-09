﻿using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Runtime;
using Application.Helpers.Web;
using Application.Mappers.GameServer;
using Application.Models.GameServer.Game;
using Application.Requests.GameServer.Game;
using Application.Responses.v1.GameServer;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Services.Integrations;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Domain.Enums.GameServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        app.MapPatch(ApiRouteConstants.GameServer.Game.Update, Update).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Game.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.Search, Search).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.DownloadLatest, DownloadLatest).ApiVersionOne();
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
    private static async Task<IResult<IEnumerable<GameSlim>>> GetAllPaginated([FromQuery]int pageNumber, [FromQuery]int pageSize, IGameService gameService,
        IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;

            var result = await gameService.GetAllPaginatedAsync(pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<GameSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount <= 0) return result;

            result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Game.GetAllPaginated, pageNumber, pageSize);
            result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Game.GetAllPaginated, pageNumber, pageSize, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            return await PaginatedResult<IEnumerable<GameSlim>>.FailAsync(ex.Message);
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
    private static async Task<IResult<GameSlim?>> GetById([FromQuery]Guid id, IGameService gameService)
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
    private static async Task<IResult<IEnumerable<GameSlim>>> GetBySteamName([FromQuery]string steamName, IGameService gameService)
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
    private static async Task<IResult<GameSlim?>> GetByFriendlyName([FromQuery]string friendlyName, IGameService gameService)
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
    private static async Task<IResult<GameSlim?>> GetBySteamGameId([FromQuery]int id, IGameService gameService)
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
    private static async Task<IResult<GameSlim?>> GetBySteamToolId([FromQuery]int id, IGameService gameService)
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
    private static async Task<IResult<Guid>> Create([FromBody]GameCreateRequest request, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.CreateAsync(request.ToCreate(), currentUserId);
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
    private static async Task<IResult> Update([FromBody]GameUpdateRequest request, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.UpdateAsync(request.ToUpdate(), currentUserId);
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
    private static async Task<IResult> Delete([FromQuery]Guid id, IGameService gameService, ICurrentUserService currentUserService)
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
    /// Search a game by its properties
    /// </summary>
    /// <param name="searchText">Text to search with</param>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of games</returns>
    /// <remarks>Searches by: ID, FriendlyName, SteamName, SteamGameId, SteamToolId, Description</remarks>
    [Authorize(PermissionConstants.GameServer.Game.Search)]
    private static async Task<IResult<IEnumerable<GameSlim>>> Search([FromQuery]string searchText, [FromQuery]int pageNumber, [FromQuery]int pageSize,
        IGameService gameService, IOptions<AppConfiguration> appConfig)
    {
        try
        {
            pageSize = pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize ? appConfig.Value.ApiPaginatedMaxPageSize : pageSize;

            var result = await gameService.SearchPaginatedAsync(searchText, pageNumber, pageSize);
            if (!result!.Succeeded)
            {
                return await PaginatedResult<IEnumerable<GameSlim>>.FailAsync(result.Messages);
            }

            if (result.TotalCount > 0)
            {
                result.Next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Game.Search, pageNumber, pageSize, result.TotalCount);
                result.Previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Game.Search, pageNumber, pageSize);
            }

            return result;
        }
        catch (Exception ex)
        {
            return await PaginatedResult<IEnumerable<GameSlim>>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Download the latest server client for a manual source game
    /// </summary>
    /// <param name="gameId">ID of the game</param>
    /// <param name="gameService"></param>
    /// <param name="fileService"></param>
    /// <returns>Information about the server client and the server client in a byte array</returns>
    /// <remarks>Only works with Games of SourceType 'Manual'</remarks>
    [Authorize(PermissionConstants.GameServer.Game.DownloadLatest)]
    private static async Task<IResult<GameDownloadResponse>> DownloadLatest([FromQuery]Guid gameId, IGameService gameService, IFileStorageRecordService fileService)
    {
        try
        {
            var foundGame = await gameService.GetByIdAsync(gameId);
            if (foundGame.Data is null)
            {
                return await Result<GameDownloadResponse>.FailAsync(ErrorMessageConstants.Games.NotFound);
            }

            if (foundGame.Data.SourceType != GameSource.Manual)
            {
                return await Result<GameDownloadResponse>.FailAsync(ErrorMessageConstants.Games.NotManualGame);
            }

            if (foundGame.Data.ManualFileRecordId is null)
            {
                return await Result<GameDownloadResponse>.FailAsync(ErrorMessageConstants.Games.NoServerClient);
            }

            var foundFile = await fileService.GetByIdAsync((Guid)foundGame.Data.ManualFileRecordId);
            if (foundFile.Data is null)
            {
                return await Result<GameDownloadResponse>.FailAsync(ErrorMessageConstants.FileStorage.NotFound);
            }

            var fileContent = await File.ReadAllBytesAsync(foundFile.Data.GetLocalFilePath());
            var response = new GameDownloadResponse
            {
                Id = foundGame.Data.Id,
                GameName = foundGame.Data.FriendlyName,
                FileName = foundFile.Data.Filename,
                Version = foundFile.Data.Version,
                HashSha256 = foundFile.Data.HashSha256,
                Content = fileContent
            };

            return await Result<GameDownloadResponse>.SuccessAsync(response);
        }
        catch (Exception ex)
        {
            return await Result<GameDownloadResponse>.FailAsync(ex.Message);
        }
    }
}