using Application.Constants.Communication;
using Application.Mappers.GameServer;
using Application.Models.GameServer.Developers;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameGenre;
using Application.Models.GameServer.Publishers;
using Application.Repositories.GameServer;
using Application.Services.GameServer;
using Application.Services.Lifecycle;
using Application.Services.System;
using Application.Settings.AppSettings;
using Domain.Contracts;
using Domain.DatabaseEntities.GameServer;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.GameServer;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IDateTimeService _dateTime;
    private readonly IRunningServerState _serverState;
    private readonly AppConfiguration _appConfig;
    private readonly IGameServerRepository _gameServerRepository;

    public GameService(IGameRepository gameRepository, IDateTimeService dateTime, IRunningServerState serverState, IOptions<AppConfiguration> appConfig,
        IGameServerRepository gameServerRepository)
    {
        _gameRepository = gameRepository;
        _dateTime = dateTime;
        _serverState = serverState;
        _gameServerRepository = gameServerRepository;
        _appConfig = appConfig.Value;
    }

    public async Task<IResult<IEnumerable<GameSlim>>> GetAllAsync()
    {
        var request = await _gameRepository.GetAllAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<GameSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameSlim>());
    }

    public async Task<IResult<IEnumerable<GameSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        var request = await _gameRepository.GetAllPaginatedAsync(pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameSlim>());
    }

    public async Task<IResult<int>> GetCountAsync()
    {
        var request = await _gameRepository.GetCountAsync();
        if (!request.Succeeded)
            return await Result<int>.FailAsync(request.ErrorMessage);

        return await Result<int>.SuccessAsync(request.Result);
    }

    public async Task<IResult<GameSlim>> GetByIdAsync(Guid id)
    {
        var request = await _gameRepository.GetByIdAsync(id);
        if (!request.Succeeded)
            return await Result<GameSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<GameSlim>>> GetBySteamNameAsync(string steamName)
    {
        var request = await _gameRepository.GetBySteamNameAsync(steamName);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameSlim>());
    }

    public async Task<IResult<GameSlim>> GetByFriendlyNameAsync(string friendlyName)
    {
        var request = await _gameRepository.GetByFriendlyNameAsync(friendlyName);
        if (!request.Succeeded)
            return await Result<GameSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<GameSlim>> GetBySteamGameIdAsync(int id)
    {
        var request = await _gameRepository.GetBySteamGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<GameSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<GameSlim>> GetBySteamToolIdAsync(int id)
    {
        var request = await _gameRepository.GetBySteamToolIdAsync(id);
        if (!request.Succeeded)
            return await Result<GameSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<Guid>> CreateAsync(GameCreate createObject)
    {
        var request = await _gameRepository.CreateAsync(createObject);
        if (!request.Succeeded)
            return await Result<Guid>.FailAsync(request.ErrorMessage);

        return await Result<Guid>.SuccessAsync(request.Result);
    }

    public async Task<IResult> UpdateAsync(GameUpdate updateObject)
    {
        var findRequest = await _gameRepository.GetByIdAsync(updateObject.Id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        updateObject.LastModifiedOn = _dateTime.NowDatabaseTime;

        var request = await _gameRepository.UpdateAsync(updateObject);
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        var findRequest = await _gameRepository.GetByIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        // Don't allow deletion if game servers are active for this game
        var assignedGameServers = await _gameServerRepository.GetByGameIdAsync(findRequest.Result.Id);
        if (assignedGameServers.Succeeded && (assignedGameServers.Result ?? Array.Empty<GameServerDb>()).Any())
        {
            List<string> errorMessages = [ErrorMessageConstants.Games.AssignedGameServers];
            errorMessages.AddRange((assignedGameServers.Result ?? Array.Empty<GameServerDb>()).ToList().Select(
                server => $"Assigned Game Server: [id]{server.Id} [name]{server.ServerName}"));
            return await Result.FailAsync(errorMessages);
        }

        // Delete all assigned game profiles for this game
        var assignedProfiles = await _gameServerRepository.GetGameProfilesByGameIdAsync(findRequest.Result.Id);
        if (assignedProfiles.Succeeded)
        {
            List<string> errorMessages = [];
            foreach (var profile in assignedProfiles.Result?.ToList() ?? [])
            {
                var profileDeleteRequest = await _gameServerRepository.DeleteGameProfileAsync(profile.Id, modifyingUserId);
                if (!profileDeleteRequest.Succeeded)
                {
                    errorMessages.Add(profileDeleteRequest.ErrorMessage);
                }
            }
            
            if (errorMessages.Count > 0)
            {
                return await Result.FailAsync(errorMessages);
            }
        }

        var deleteRequest = await _gameRepository.DeleteAsync(id, modifyingUserId);
        if (!deleteRequest.Succeeded)
            return await Result.FailAsync(deleteRequest.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<GameSlim>>> SearchAsync(string searchText)
    {
        var request = await _gameRepository.SearchAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameSlim>());
    }

    public async Task<IResult<IEnumerable<GameSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var request = await _gameRepository.SearchPaginatedAsync(searchText, pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameSlim>());
    }

    public async Task<IResult<IEnumerable<DeveloperSlim>>> GetAllDevelopersAsync()
    {
        var request = await _gameRepository.GetAllDevelopersAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<DeveloperSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<DeveloperSlim>());
    }

    public async Task<IResult<IEnumerable<DeveloperSlim>>> GetAllDevelopersPaginatedAsync(int pageNumber, int pageSize)
    {
        var request = await _gameRepository.GetAllDevelopersPaginatedAsync(pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<DeveloperSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<DeveloperSlim>());
    }

    public async Task<IResult<int>> GetDevelopersCountAsync()
    {
        var request = await _gameRepository.GetDevelopersCountAsync();
        if (!request.Succeeded)
            return await Result<int>.FailAsync(request.ErrorMessage);

        return await Result<int>.SuccessAsync(request.Result);
    }

    public async Task<IResult<DeveloperSlim>> GetDeveloperByIdAsync(Guid id)
    {
        var request = await _gameRepository.GetDeveloperByIdAsync(id);
        if (!request.Succeeded)
            return await Result<DeveloperSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<DeveloperSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<DeveloperSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<DeveloperSlim>> GetDeveloperByNameAsync(string name)
    {
        var request = await _gameRepository.GetDeveloperByNameAsync(name);
        if (!request.Succeeded)
            return await Result<DeveloperSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<DeveloperSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<DeveloperSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<DeveloperSlim>>> GetDevelopersByGameIdAsync(Guid id)
    {
        var request = await _gameRepository.GetDevelopersByGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<DeveloperSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<DeveloperSlim>());
    }

    public async Task<IResult<Guid>> CreateDeveloperAsync(DeveloperCreate createObject)
    {
        var request = await _gameRepository.CreateDeveloperAsync(createObject);
        if (!request.Succeeded)
            return await Result<Guid>.FailAsync(request.ErrorMessage);

        return await Result<Guid>.SuccessAsync(request.Result);
    }

    public async Task<IResult> DeleteDeveloperAsync(Guid id)
    {
        var findRequest = await _gameRepository.GetDevelopersByGameIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);
        
        var request = await _gameRepository.DeleteDeveloperAsync(id);
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<DeveloperSlim>>> SearchDevelopersAsync(string searchText)
    {
        var request = await _gameRepository.SearchDevelopersAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<DeveloperSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<DeveloperSlim>());
    }

    public async Task<IResult<IEnumerable<DeveloperSlim>>> SearchDevelopersPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var request = await _gameRepository.SearchDevelopersPaginatedAsync(searchText, pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<DeveloperSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<DeveloperSlim>());
    }

    public async Task<IResult<IEnumerable<PublisherSlim>>> GetAllPublishersAsync()
    {
        var request = await _gameRepository.GetAllPublishersAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<PublisherSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<PublisherSlim>());
    }

    public async Task<IResult<IEnumerable<PublisherSlim>>> GetAllPublishersPaginatedAsync(int pageNumber, int pageSize)
    {
        var request = await _gameRepository.GetAllPublishersPaginatedAsync(pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<PublisherSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<PublisherSlim>());
    }

    public async Task<IResult<int>> GetPublishersCountAsync()
    {
        var request = await _gameRepository.GetPublishersCountAsync();
        if (!request.Succeeded)
            return await Result<int>.FailAsync(request.ErrorMessage);

        return await Result<int>.SuccessAsync(request.Result);
    }

    public async Task<IResult<PublisherSlim>> GetPublisherByIdAsync(Guid id)
    {
        var request = await _gameRepository.GetPublisherByIdAsync(id);
        if (!request.Succeeded)
            return await Result<PublisherSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<PublisherSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<PublisherSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<PublisherSlim>> GetPublisherByNameAsync(string name)
    {
        var request = await _gameRepository.GetPublisherByNameAsync(name);
        if (!request.Succeeded)
            return await Result<PublisherSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<PublisherSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<PublisherSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<PublisherSlim>>> GetPublishersByGameIdAsync(Guid id)
    {
        var request = await _gameRepository.GetPublishersByGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<PublisherSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<PublisherSlim>());
    }

    public async Task<IResult<Guid>> CreatePublisherAsync(PublisherCreate createObject)
    {
        var request = await _gameRepository.CreatePublisherAsync(createObject);
        if (!request.Succeeded)
            return await Result<Guid>.FailAsync(request.ErrorMessage);

        return await Result<Guid>.SuccessAsync(request.Result);
    }

    public async Task<IResult> DeletePublisherAsync(Guid id)
    {
        var findRequest = await _gameRepository.GetPublisherByIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var request = await _gameRepository.DeletePublisherAsync(id);
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<PublisherSlim>>> SearchPublishersAsync(string searchText)
    {
        var request = await _gameRepository.SearchPublishersAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<PublisherSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<PublisherSlim>());
    }

    public async Task<IResult<IEnumerable<PublisherSlim>>> SearchPublishersPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var request = await _gameRepository.SearchPublishersPaginatedAsync(searchText, pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<PublisherSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<PublisherSlim>());
    }

    public async Task<IResult<IEnumerable<GameGenreSlim>>> GetAllGameGenresAsync()
    {
        var request = await _gameRepository.GetAllGameGenresAsync();
        if (!request.Succeeded)
            return await Result<IEnumerable<GameGenreSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameGenreSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameGenreSlim>());
    }

    public async Task<IResult<IEnumerable<GameGenreSlim>>> GetAllGameGenresPaginatedAsync(int pageNumber, int pageSize)
    {
        var request = await _gameRepository.GetAllGameGenresPaginatedAsync(pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameGenreSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameGenreSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameGenreSlim>());
    }

    public async Task<IResult<int>> GetGameGenresCountAsync()
    {
        var request = await _gameRepository.GetGameGenresCountAsync();
        if (!request.Succeeded)
            return await Result<int>.FailAsync(request.ErrorMessage);

        return await Result<int>.SuccessAsync(request.Result);
    }

    public async Task<IResult<GameGenreSlim>> GetGameGenreByIdAsync(Guid id)
    {
        var request = await _gameRepository.GetGameGenreByIdAsync(id);
        if (!request.Succeeded)
            return await Result<GameGenreSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameGenreSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameGenreSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<GameGenreSlim>> GetGameGenreByNameAsync(string name)
    {
        var request = await _gameRepository.GetGameGenreByNameAsync(name);
        if (!request.Succeeded)
            return await Result<GameGenreSlim>.FailAsync(request.ErrorMessage);
        if (request.Result is null)
            return await Result<GameGenreSlim>.FailAsync(ErrorMessageConstants.Generic.NotFound);

        return await Result<GameGenreSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<GameGenreSlim>>> GetGameGenresByGameIdAsync(Guid id)
    {
        var request = await _gameRepository.GetGameGenresByGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameGenreSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameGenreSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameGenreSlim>());
    }

    public async Task<IResult<Guid>> CreateGameGenreAsync(GameGenreCreate createObject)
    {
        var request = await _gameRepository.CreateGameGenreAsync(createObject);
        if (!request.Succeeded)
            return await Result<Guid>.FailAsync(request.ErrorMessage);

        return await Result<Guid>.SuccessAsync(request.Result);
    }

    public async Task<IResult> DeleteGameGenreAsync(Guid id)
    {
        var findRequest = await _gameRepository.GetGameGenreByIdAsync(id);
        if (!findRequest.Succeeded || findRequest.Result is null)
            return await Result.FailAsync(ErrorMessageConstants.Generic.NotFound);

        var request = await _gameRepository.DeleteGameGenreAsync(id);
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<GameGenreSlim>>> SearchGameGenresAsync(string searchText)
    {
        var request = await _gameRepository.SearchGameGenresAsync(searchText);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameGenreSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameGenreSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameGenreSlim>());
    }

    public async Task<IResult<IEnumerable<GameGenreSlim>>> SearchGameGenresPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var request = await _gameRepository.SearchGameGenresPaginatedAsync(searchText, pageNumber, pageSize);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameGenreSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameGenreSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameGenreSlim>());
    }
}