using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.Developers;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameGenre;
using Application.Models.GameServer.Publishers;
using Application.Requests.GameServer.Game;
using Application.Services.GameServer;
using Application.Services.Identity;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
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
        app.MapPost(ApiRouteConstants.GameServer.Game.Update, Update).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Game.Delete, Delete).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.Search, Search).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Developer.GetAll, GetAllDevelopersPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Developer.GetCount, GetDevelopersCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Developer.GetById, GetDeveloperById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Developer.GetByName, GetDeveloperByName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Developer.GetByGameId, GetDevelopersByGameId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Developer.Create, CreateDeveloper).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Developer.Delete, DeleteDeveloper).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Developer.Search, SearchDevelopers).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Publisher.GetAll, GetAllPublishersPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Publisher.GetCount, GetPublishersCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Publisher.GetById, GetPublisherById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Publisher.GetByName, GetPublisherByName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Publisher.GetByGameId, GetPublishersByGameId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Publisher.Create, CreatePublisher).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Publisher.Delete, DeletePublisher).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Publisher.Search, SearchPublishers).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.GetAll, GetAllGameGenresPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.GetCount, GetGameGenresCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.GetById, GetGameGenreById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.GetByName, GetGameGenreByName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.GetByGameId, GetGameGenresByGameId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.GameGenre.Create, CreateGameGenre).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.GameGenre.Delete, DeleteGameGenre).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.GameGenre.Search, SearchGameGenres).ApiVersionOne();
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
    private static async Task<IResult<IEnumerable<GameSlim>>> GetAllPaginated(int pageNumber, int pageSize, IGameService gameService,
        IOptions<AppConfiguration> appConfig)
    {
        try
        {
            if (pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize) pageSize = 500;

            var result = await gameService.GetAllPaginatedAsync(pageNumber, pageSize);
            if (!result.Succeeded)
                return await Result<IEnumerable<GameSlim>>.FailAsync(result.Messages);
            
            var totalCountRequest = await gameService.GetCountAsync();
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Game.GetAllPaginated,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Game.GetAllPaginated,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<GameSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameSlim>>.FailAsync(ex.Message);
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
    private static async Task<IResult<GameSlim>> GetById(Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game by steam name
    /// </summary>
    /// <param name="steamName">Game steam name</param>
    /// <param name="gameService"></param>
    /// <returns>Game object</returns>
    [Authorize(PermissionConstants.GameServer.Game.Get)]
    private static async Task<IResult<IEnumerable<GameSlim>>> GetBySteamName(string steamName, IGameService gameService)
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
    private static async Task<IResult<GameSlim>> GetByFriendlyName(string friendlyName, IGameService gameService)
    {
        try
        {
            return await gameService.GetByFriendlyNameAsync(friendlyName);
        }
        catch (Exception ex)
        {
            return await Result<GameSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game by steam id
    /// </summary>
    /// <param name="id">Game steam id</param>
    /// <param name="gameService"></param>
    /// <returns>Game object</returns>
    [Authorize(PermissionConstants.GameServer.Game.Get)]
    private static async Task<IResult<GameSlim>> GetBySteamGameId(int id, IGameService gameService)
    {
        try
        {
            return await gameService.GetBySteamGameIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game by steam tool id
    /// </summary>
    /// <param name="id">Game steam tool id</param>
    /// <param name="gameService"></param>
    /// <returns>Game object</returns>
    [Authorize(PermissionConstants.GameServer.Game.Get)]
    private static async Task<IResult<GameSlim>> GetBySteamToolId(int id, IGameService gameService)
    {
        try
        {
            return await gameService.GetBySteamToolIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameSlim>.FailAsync(ex.Message);
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
    private static async Task<IResult<Guid>> Create(GameCreateRequest request, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.CreateAsync(request, currentUserId);
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
    private static async Task<IResult> Update(GameUpdateRequest request, IGameService gameService, ICurrentUserService currentUserService)
    {
        try
        {
            var currentUserId = await currentUserService.GetApiCurrentUserId();
            return await gameService.UpdateAsync(request, currentUserId);
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
    private static async Task<IResult> Delete(Guid id, IGameService gameService, ICurrentUserService currentUserService)
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
    /// Search a game by it's properties
    /// </summary>
    /// <param name="searchText">Text to search with</param>
    /// <param name="gameService"></param>
    /// <returns>List of games</returns>
    /// <remarks>Searches by: FriendlyName, SteamName, SteamGameId, SteamToolId, Description</remarks>
    [Authorize(PermissionConstants.GameServer.Game.Search)]
    private static async Task<IResult<IEnumerable<GameSlim>>> Search(string searchText, IGameService gameService)
    {
        try
        {
            return await gameService.SearchAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameSlim>>.FailAsync(ex.Message);
        }
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
    private static async Task<IResult<IEnumerable<DeveloperSlim>>> GetAllDevelopersPaginated(int pageNumber, int pageSize, IGameService gameService,
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
    private static async Task<IResult<int>> GetDevelopersCount(IGameService gameService)
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
    private static async Task<IResult<DeveloperSlim>> GetDeveloperById(Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.GetDeveloperByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<DeveloperSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a developer by name
    /// </summary>
    /// <param name="name">Developer name</param>
    /// <param name="gameService"></param>
    /// <returns>Developer object</returns>
    [Authorize(PermissionConstants.GameServer.Developer.Get)]
    private static async Task<IResult<DeveloperSlim>> GetDeveloperByName(string name, IGameService gameService)
    {
        try
        {
            return await gameService.GetDeveloperByNameAsync(name);
        }
        catch (Exception ex)
        {
            return await Result<DeveloperSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get developers by game id
    /// </summary>
    /// <param name="id">Game id</param>
    /// <param name="gameService"></param>
    /// <returns>List of developers</returns>
    [Authorize(PermissionConstants.GameServer.Developer.Get)]
    private static async Task<IResult<IEnumerable<DeveloperSlim>>> GetDevelopersByGameId(Guid id, IGameService gameService)
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
    private static async Task<IResult<Guid>> CreateDeveloper(DeveloperCreate createObject, IGameService gameService, ICurrentUserService currentUserService)
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
    private static async Task<IResult> DeleteDeveloper(Guid id, IGameService gameService, ICurrentUserService currentUserService)
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
    private static async Task<IResult<IEnumerable<DeveloperSlim>>> SearchDevelopers(string searchText, IGameService gameService)
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
    
    /// <summary>
    /// Get all publishers with pagination
    /// </summary>
    /// <param name="pageNumber">Page number to get</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="gameService"></param>
    /// <param name="appConfig"></param>
    /// <returns>List of publishers</returns>
    [Authorize(PermissionConstants.GameServer.Publisher.GetAllPaginated)]
    private static async Task<IResult<IEnumerable<PublisherSlim>>> GetAllPublishersPaginated(int pageNumber, int pageSize, IGameService gameService,
        IOptions<AppConfiguration> appConfig)
    {
        try
        {
            if (pageSize < 0 || pageSize > appConfig.Value.ApiPaginatedMaxPageSize) pageSize = 500;

            var result = await gameService.GetAllPublishersPaginatedAsync(pageNumber, pageSize);
            if (!result.Succeeded)
                return await Result<IEnumerable<PublisherSlim>>.FailAsync(result.Messages);
            
            var totalCountRequest = await gameService.GetPublishersCountAsync();
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Publisher.GetAll,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Publisher.GetAll,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<PublisherSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
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
    private static async Task<IResult<int>> GetPublishersCount(IGameService gameService)
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
    private static async Task<IResult<PublisherSlim>> GetPublisherById(Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.GetPublisherByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<PublisherSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a publisher by name
    /// </summary>
    /// <param name="name">Publisher name</param>
    /// <param name="gameService"></param>
    /// <returns>Publisher object</returns>
    [Authorize(PermissionConstants.GameServer.Publisher.Get)]
    private static async Task<IResult<PublisherSlim>> GetPublisherByName(string name, IGameService gameService)
    {
        try
        {
            return await gameService.GetPublisherByNameAsync(name);
        }
        catch (Exception ex)
        {
            return await Result<PublisherSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get publishers by game id
    /// </summary>
    /// <param name="id">Game id</param>
    /// <param name="gameService"></param>
    /// <returns>List of publishers</returns>
    [Authorize(PermissionConstants.GameServer.Publisher.Get)]
    private static async Task<IResult<IEnumerable<PublisherSlim>>> GetPublishersByGameId(Guid id, IGameService gameService)
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
    private static async Task<IResult<Guid>> CreatePublisher(PublisherCreate createObject, IGameService gameService, ICurrentUserService currentUserService)
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
    private static async Task<IResult> DeletePublisher(Guid id, IGameService gameService, ICurrentUserService currentUserService)
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
    /// <param name="gameService"></param>
    /// <returns>List of publishers</returns>
    /// <remarks>Searches by: Name</remarks>
    [Authorize(PermissionConstants.GameServer.Publisher.Search)]
    private static async Task<IResult<IEnumerable<PublisherSlim>>> SearchPublishers(string searchText, IGameService gameService)
    {
        try
        {
            return await gameService.SearchPublishersAsync(searchText);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(ex.Message);
        }
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
    private static async Task<IResult<IEnumerable<GameGenreSlim>>> GetAllGameGenresPaginated(int pageNumber, int pageSize, IGameService gameService,
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
    private static async Task<IResult<int>> GetGameGenresCount(IGameService gameService)
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
    private static async Task<IResult<GameGenreSlim>> GetGameGenreById(Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.GetGameGenreByIdAsync(id);
        }
        catch (Exception ex)
        {
            return await Result<GameGenreSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get a game genre by name
    /// </summary>
    /// <param name="name">Game genre name</param>
    /// <param name="gameService"></param>
    /// <returns>Game genre object</returns>
    [Authorize(PermissionConstants.GameServer.GameGenre.Get)]
    private static async Task<IResult<GameGenreSlim>> GetGameGenreByName(string name, IGameService gameService)
    {
        try
        {
            return await gameService.GetGameGenreByNameAsync(name);
        }
        catch (Exception ex)
        {
            return await Result<GameGenreSlim>.FailAsync(ex.Message);
        }
    }
    
    /// <summary>
    /// Get game genres by game id
    /// </summary>
    /// <param name="id">Game id</param>
    /// <param name="gameService"></param>
    /// <returns>List of game genres</returns>
    [Authorize(PermissionConstants.GameServer.GameGenre.Get)]
    private static async Task<IResult<IEnumerable<GameGenreSlim>>> GetGameGenresByGameId(Guid id, IGameService gameService)
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
    private static async Task<IResult<Guid>> CreateGameGenre(GameGenreCreate createObject, IGameService gameService, ICurrentUserService currentUserService)
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
    private static async Task<IResult> DeleteGameGenre(Guid id, IGameService gameService, ICurrentUserService currentUserService)
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
    private static async Task<IResult<IEnumerable<GameGenreSlim>>> SearchGameGenres(string searchText, IGameService gameService)
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