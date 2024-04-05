using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.Developers;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameGenre;
using Application.Models.GameServer.Publishers;
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
        app.MapGet(ApiRouteConstants.GameServer.Game.GetAll, GetAll).ApiVersionOne();
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
        app.MapGet(ApiRouteConstants.GameServer.Game.GetAllDevelopers, GetAllDevelopers).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetAllDevelopersPaginated, GetAllDevelopersPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetDevelopersCount, GetDevelopersCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetDeveloperById, GetDeveloperById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetDeveloperByName, GetDeveloperByName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetDevelopersByGameId, GetDevelopersByGameId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Game.CreateDeveloper, CreateDeveloper).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Game.DeleteDeveloper, DeleteDeveloper).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.SearchDevelopers, SearchDevelopers).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetAllPublishers, GetAllPublishers).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetAllPublishersPaginated, GetAllPublishersPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetPublishersCount, GetPublishersCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetPublisherById, GetPublisherById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetPublisherByName, GetPublisherByName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetPublishersByGameId, GetPublishersByGameId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Game.CreatePublisher, CreatePublisher).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Game.DeletePublisher, DeletePublisher).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.SearchPublishers, SearchPublishers).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetAllGameGenres, GetAllGameGenres).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetAllGameGenresPaginated, GetAllGameGenresPaginated).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetGameGenresCount, GetGameGenresCount).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetGameGenreById, GetGameGenreById).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetGameGenreByName, GetGameGenreByName).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.GetGameGenresByGameId, GetGameGenresByGameId).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Game.CreateGameGenre, CreateGameGenre).ApiVersionOne();
        app.MapDelete(ApiRouteConstants.GameServer.Game.DeleteGameGenre, DeleteGameGenre).ApiVersionOne();
        app.MapGet(ApiRouteConstants.GameServer.Game.SearchGameGenres, SearchGameGenres).ApiVersionOne();
    }


    [Authorize(PermissionConstants.Game.GetAll)]
    private static async Task<IResult<IEnumerable<GameSlim>>> GetAll(IGameService gameService)
    {
        try
        {
            return await gameService.GetAllAsync();
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameSlim>>.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.GetAllPaginated)]
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


    [Authorize(PermissionConstants.Game.GetCount)]
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


    [Authorize(PermissionConstants.Game.Get)]
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


    [Authorize(PermissionConstants.Game.Get)]
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


    [Authorize(PermissionConstants.Game.Get)]
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


    [Authorize(PermissionConstants.Game.Get)]
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


    [Authorize(PermissionConstants.Game.Get)]
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


    [Authorize(PermissionConstants.Game.Create)]
    private static async Task<IResult<Guid>> Create(GameCreate createObject, IGameService gameService)
    {
        try
        {
            return await gameService.CreateAsync(createObject);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.Update)]
    private static async Task<IResult> Update(GameUpdate updateObject, IGameService gameService)
    {
        try
        {
            return await gameService.UpdateAsync(updateObject);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.Delete)]
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


    [Authorize(PermissionConstants.Game.Search)]
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


    [Authorize(PermissionConstants.Game.GetAllDevelopers)]
    private static async Task<IResult<IEnumerable<DeveloperSlim>>> GetAllDevelopers(IGameService gameService)
    {
        try
        {
            return await gameService.GetAllDevelopersAsync();
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.GetAllDevelopersPaginated)]
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
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Game.GetAllDevelopersPaginated,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Game.GetAllDevelopersPaginated,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<DeveloperSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.GetDevelopersCount)]
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


    [Authorize(PermissionConstants.Game.GetDeveloper)]
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


    [Authorize(PermissionConstants.Game.GetDeveloper)]
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


    [Authorize(PermissionConstants.Game.GetDevelopers)]
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


    [Authorize(PermissionConstants.Game.CreateDeveloper)]
    private static async Task<IResult<Guid>> CreateDeveloper(DeveloperCreate createObject, IGameService gameService)
    {
        try
        {
            return await gameService.CreateDeveloperAsync(createObject);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.DeleteDeveloper)]
    private static async Task<IResult> DeleteDeveloper(Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.DeleteDeveloperAsync(id);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.SearchDevelopers)]
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


    [Authorize(PermissionConstants.Game.GetAllPublishers)]
    private static async Task<IResult<IEnumerable<PublisherSlim>>> GetAllPublishers(IGameService gameService)
    {
        try
        {
            return await gameService.GetAllPublishersAsync();
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.GetAllPublishersPaginated)]
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
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Game.GetAllPublishersPaginated,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Game.GetAllPublishersPaginated,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<PublisherSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.GetPublishersCount)]
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


    [Authorize(PermissionConstants.Game.GetPublisher)]
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


    [Authorize(PermissionConstants.Game.GetPublisher)]
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


    [Authorize(PermissionConstants.Game.GetPublishers)]
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


    [Authorize(PermissionConstants.Game.CreatePublisher)]
    private static async Task<IResult<Guid>> CreatePublisher(PublisherCreate createObject, IGameService gameService)
    {
        try
        {
            return await gameService.CreatePublisherAsync(createObject);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.DeletePublisher)]
    private static async Task<IResult> DeletePublisher(Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.DeletePublisherAsync(id);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.SearchPublishers)]
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


    [Authorize(PermissionConstants.Game.GetAllGameGenres)]
    private static async Task<IResult<IEnumerable<GameGenreSlim>>> GetAllGameGenres(IGameService gameService)
    {
        try
        {
            return await gameService.GetAllGameGenresAsync();
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameGenreSlim>>.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.GetAllGameGenresPaginated)]
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
            var previous = appConfig.Value.BaseUrl.GetPaginatedPreviousUrl(ApiRouteConstants.GameServer.Game.GetAllGameGenres,
                pageNumber, pageSize);
            var next = appConfig.Value.BaseUrl.GetPaginatedNextUrl(ApiRouteConstants.GameServer.Game.GetAllGameGenres,
                pageNumber, pageSize, totalCountRequest.Data);
            
            return await PaginatedResult<IEnumerable<GameGenreSlim>>.SuccessAsync(result.Data, pageNumber, totalCountRequest.Data, pageSize, previous, next);
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameGenreSlim>>.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.GetGameGenresCount)]
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


    [Authorize(PermissionConstants.Game.GetGameGenre)]
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


    [Authorize(PermissionConstants.Game.GetGameGenre)]
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


    [Authorize(PermissionConstants.Game.GetGameGenres)]
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


    [Authorize(PermissionConstants.Game.CreateGameGenre)]
    private static async Task<IResult<Guid>> CreateGameGenre(GameGenreCreate createObject, IGameService gameService)
    {
        try
        {
            return await gameService.CreateGameGenreAsync(createObject);
        }
        catch (Exception ex)
        {
            return await Result<Guid>.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.DeleteGameGenre)]
    private static async Task<IResult> DeleteGameGenre(Guid id, IGameService gameService)
    {
        try
        {
            return await gameService.DeleteGameGenreAsync(id);
        }
        catch (Exception ex)
        {
            return await Result.FailAsync(ex.Message);
        }
    }


    [Authorize(PermissionConstants.Game.SearchGameGenres)]
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