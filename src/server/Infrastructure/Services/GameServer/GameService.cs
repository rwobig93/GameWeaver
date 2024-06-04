using Application.Constants.Communication;
using Application.Helpers.Lifecycle;
using Application.Mappers.GameServer;
using Application.Models.Events;
using Application.Models.GameServer.Developers;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameGenre;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameUpdate;
using Application.Models.GameServer.Publishers;
using Application.Repositories.GameServer;
using Application.Repositories.Lifecycle;
using Application.Requests.GameServer.Game;
using Application.Services.GameServer;
using Application.Services.System;
using Domain.Contracts;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.Database;
using Domain.Enums.Lifecycle;

namespace Infrastructure.Services.GameServer;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IDateTimeService _dateTime;
    private readonly IGameServerRepository _gameServerRepository;
    private readonly IEventService _eventService;
    private readonly IAuditTrailsRepository _auditRepository;

    public GameService(IGameRepository gameRepository, IDateTimeService dateTime, IGameServerRepository gameServerRepository, IEventService eventService,
        IAuditTrailsRepository auditRepository)
    {
        _gameRepository = gameRepository;
        _dateTime = dateTime;
        _gameServerRepository = gameServerRepository;
        _eventService = eventService;
        _auditRepository = auditRepository;
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
        if (request.Result is null)
        {
            return await Result<GameSlim>.FailAsync(ErrorMessageConstants.Games.NotFound);
        }

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
        if (request.Result is null)
        {
            return await Result<GameSlim>.FailAsync(ErrorMessageConstants.Games.NotFound);
        }

        return await Result<GameSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<GameSlim>> GetBySteamGameIdAsync(int id)
    {
        var request = await _gameRepository.GetBySteamGameIdAsync(id);
        if (request.Result is null)
        {
            return await Result<GameSlim>.FailAsync(ErrorMessageConstants.Games.NotFound);
        }

        return await Result<GameSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<GameSlim>> GetBySteamToolIdAsync(int id)
    {
        var request = await _gameRepository.GetBySteamToolIdAsync(id);
        if (request.Result is null)
        {
            return await Result<GameSlim>.FailAsync(ErrorMessageConstants.Games.NotFound);
        }

        return await Result<GameSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<Guid>> CreateAsync(GameCreateRequest request, Guid requestUserId)
    {
        if (request.SteamToolId == 0)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Games.InvalidSteamToolId);
        }
        
        var matchingGame = await _gameRepository.GetBySteamToolIdAsync(request.SteamToolId);
        if (matchingGame.Result is not null)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Games.DuplicateSteamToolId);
        }
        
        var convertedRequest = request.ToCreate();
        convertedRequest.CreatedBy = requestUserId;
        convertedRequest.CreatedOn = _dateTime.NowDatabaseTime;
        convertedRequest.DefaultGameProfileId = Guid.Empty;
        
        var createRequest = await _gameRepository.CreateAsync(convertedRequest);
        if (!createRequest.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGames, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to create a game"},
                {"Error", createRequest.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var defaultProfileCreate = await _gameServerRepository.CreateGameProfileAsync(new GameProfileCreate
        {
            FriendlyName = $"{convertedRequest.FriendlyName} - Default",
            OwnerId = requestUserId,
            GameId = createRequest.Result,
            CreatedBy = requestUserId,
            CreatedOn = _dateTime.NowDatabaseTime
        });
        if (!defaultProfileCreate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGames, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"GameId", createRequest.Result.ToString()},
                {"Detail", "Successfully created game but failed to create default profile"},
                {"Error", defaultProfileCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var updateGameProfile = await _gameRepository.UpdateAsync(new GameUpdate {Id = createRequest.Result, DefaultGameProfileId = defaultProfileCreate.Result});
        if (!updateGameProfile.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGames, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"GameId", createRequest.Result.ToString()},
                {"Detail", "Successfully created game and default profile, failed to assign default profile to the game"},
                {"Error", updateGameProfile.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var createdGame = await _gameRepository.GetByIdAsync(createRequest.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Games, createRequest.Result, requestUserId, DatabaseActionType.Create,
            null, createdGame.Result);

        return await Result<Guid>.SuccessAsync(createRequest.Result);
    }

    public async Task<IResult> UpdateAsync(GameUpdateRequest request, Guid requestUserId)
    {
        var foundGame = await _gameRepository.GetByIdAsync(request.Id);
        if (foundGame.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.Games.NotFound);
        }

        var convertedRequest = request.ToUpdate();
        convertedRequest.LastModifiedOn = _dateTime.NowDatabaseTime;
        convertedRequest.LastModifiedBy = requestUserId;

        var updateRequest = await _gameRepository.UpdateAsync(convertedRequest);
        if (!updateRequest.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGames, foundGame.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to update game"},
                {"Error", updateRequest.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var updatedGame = await _gameRepository.GetByIdAsync(foundGame.Result.Id);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Games, foundGame.Result.Id, requestUserId, DatabaseActionType.Update,
            foundGame.Result, updatedGame.Result);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteAsync(Guid id, Guid requestUserId)
    {
        var foundGame = await _gameRepository.GetByIdAsync(id);
        if (!foundGame.Succeeded || foundGame.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.Games.NotFound);
        }

        // Don't allow deletion if game servers are active for this game
        var assignedGameServers = await _gameServerRepository.GetByGameIdAsync(foundGame.Result.Id);
        if (assignedGameServers.Succeeded && (assignedGameServers.Result ?? Array.Empty<GameServerDb>()).Any())
        {
            List<string> errorMessages = [ErrorMessageConstants.Games.AssignedGameServers];
            errorMessages.AddRange((assignedGameServers.Result ?? Array.Empty<GameServerDb>()).ToList().Select(
                server => $"Assigned Game Server: [id]{server.Id} [name]{server.ServerName}"));
            return await Result.FailAsync(errorMessages);
        }

        // Delete all assigned game profiles for this game
        var assignedProfiles = await _gameServerRepository.GetGameProfilesByGameIdAsync(foundGame.Result.Id);
        if (assignedProfiles.Succeeded)
        {
            List<string> errorMessages = [];
            foreach (var profile in assignedProfiles.Result?.ToList() ?? [])
            {
                var profileDeleteRequest = await _gameServerRepository.DeleteGameProfileAsync(profile.Id, requestUserId);
                if (profileDeleteRequest.Succeeded) continue;
                
                errorMessages.Add(profileDeleteRequest.ErrorMessage);
            }
            
            if (errorMessages.Count > 0)
            {
                return await Result.FailAsync(errorMessages);
            }
        }
        
        // Delete all update records for this game
        var deleteUpdatesRequest = await _gameRepository.DeleteGameUpdatesForGameIdAsync(foundGame.Result.Id);
        if (!deleteUpdatesRequest.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGames, foundGame.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Deleted any assigned profiles for game but failed to delete game update records before deleting the game"},
                {"Error", deleteUpdatesRequest.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var deleteRequest = await _gameRepository.DeleteAsync(id, requestUserId);
        if (!deleteRequest.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGames, foundGame.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Deleted any assigned profiles and update records but failed to delete the game"},
                {"Error", deleteRequest.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Games, foundGame.Result.Id, requestUserId, DatabaseActionType.Delete, foundGame.Result);

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
        if (request.Result is null)
        {
            return await Result<DeveloperSlim>.FailAsync(ErrorMessageConstants.Developers.NotFound);
        }

        return await Result<DeveloperSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<DeveloperSlim>> GetDeveloperByNameAsync(string name)
    {
        var request = await _gameRepository.GetDeveloperByNameAsync(name);
        if (request.Result is null)
        {
            return await Result<DeveloperSlim>.FailAsync(ErrorMessageConstants.Developers.NotFound);
        }

        return await Result<DeveloperSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<DeveloperSlim>>> GetDevelopersByGameIdAsync(Guid id)
    {
        var request = await _gameRepository.GetDevelopersByGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<DeveloperSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<DeveloperSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<DeveloperSlim>());
    }

    public async Task<IResult<Guid>> CreateDeveloperAsync(DeveloperCreate request, Guid requestUserId)
    {
        var developerCreate = await _gameRepository.CreateDeveloperAsync(request);
        if (!developerCreate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootDevelopers, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to create developer"},
                {"Error", developerCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var createdDeveloper = await _gameRepository.GetDeveloperByIdAsync(developerCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Developers, developerCreate.Result, requestUserId, DatabaseActionType.Create, 
            null, createdDeveloper.Result);

        return await Result<Guid>.SuccessAsync(developerCreate.Result);
    }

    public async Task<IResult> DeleteDeveloperAsync(Guid id, Guid requestUserId)
    {
        var foundDeveloper = await _gameRepository.GetDeveloperByIdAsync(id);
        if (foundDeveloper.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.Developers.NotFound);
        }

        var deleteDeveloper = await _gameRepository.DeleteDeveloperAsync(id);
        if (!deleteDeveloper.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootDevelopers, foundDeveloper.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to delete developer"},
                {"Error", deleteDeveloper.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Developers, foundDeveloper.Result.Id, requestUserId, DatabaseActionType.Delete,
            foundDeveloper.Result);

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
        if (request.Result is null)
        {
            return await Result<PublisherSlim>.FailAsync(ErrorMessageConstants.Publishers.NotFound);
        }

        return await Result<PublisherSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<PublisherSlim>> GetPublisherByNameAsync(string name)
    {
        var request = await _gameRepository.GetPublisherByNameAsync(name);
        if (request.Result is null)
        {
            return await Result<PublisherSlim>.FailAsync(ErrorMessageConstants.Publishers.NotFound);
        }

        return await Result<PublisherSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<PublisherSlim>>> GetPublishersByGameIdAsync(Guid id)
    {
        var request = await _gameRepository.GetPublishersByGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<PublisherSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<PublisherSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<PublisherSlim>());
    }

    public async Task<IResult<Guid>> CreatePublisherAsync(PublisherCreate request, Guid requestUserId)
    {
        var publisherCreate = await _gameRepository.CreatePublisherAsync(request);
        if (!publisherCreate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootPublishers, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to create publisher"},
                {"Error", publisherCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var createdPublisher = await _gameRepository.GetPublisherByIdAsync(publisherCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Publishers, publisherCreate.Result, requestUserId, DatabaseActionType.Create,
            null, createdPublisher.Result);

        return await Result<Guid>.SuccessAsync(publisherCreate.Result);
    }

    public async Task<IResult> DeletePublisherAsync(Guid id, Guid requestUserId)
    {
        var foundPublisher = await _gameRepository.GetPublisherByIdAsync(id);
        if (foundPublisher.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.Publishers.NotFound);
        }

        var deletePublisher = await _gameRepository.DeletePublisherAsync(id);
        if (!deletePublisher.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootPublishers, foundPublisher.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to delete publisher"},
                {"Error", deletePublisher.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Publishers, foundPublisher.Result.Id, requestUserId, DatabaseActionType.Delete,
            foundPublisher.Result);

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
        if (request.Result is null)
        {
            return await Result<GameGenreSlim>.FailAsync(ErrorMessageConstants.GameGenres.NotFound);
        }

        return await Result<GameGenreSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<GameGenreSlim>> GetGameGenreByNameAsync(string name)
    {
        var request = await _gameRepository.GetGameGenreByNameAsync(name);
        if (request.Result is null)
        {
            return await Result<GameGenreSlim>.FailAsync(ErrorMessageConstants.GameGenres.NotFound);
        }

        return await Result<GameGenreSlim>.SuccessAsync(request.Result.ToSlim());
    }

    public async Task<IResult<IEnumerable<GameGenreSlim>>> GetGameGenresByGameIdAsync(Guid id)
    {
        var request = await _gameRepository.GetGameGenresByGameIdAsync(id);
        if (!request.Succeeded)
            return await Result<IEnumerable<GameGenreSlim>>.FailAsync(request.ErrorMessage);

        return await Result<IEnumerable<GameGenreSlim>>.SuccessAsync(request.Result?.ToSlims() ?? new List<GameGenreSlim>());
    }

    public async Task<IResult<Guid>> CreateGameGenreAsync(GameGenreCreate createObject, Guid requestUserId)
    {
        var genreCreate = await _gameRepository.CreateGameGenreAsync(createObject);
        if (!genreCreate.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameGenres, Guid.Empty, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to create game genre"},
                {"Error", genreCreate.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }

        var createdGenre = await _gameRepository.GetGameGenreByIdAsync(genreCreate.Result);
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameGenres, genreCreate.Result, requestUserId, DatabaseActionType.Create,
            null, createdGenre.Result);

        return await Result<Guid>.SuccessAsync(genreCreate.Result);
    }

    public async Task<IResult> DeleteGameGenreAsync(Guid id, Guid requestUserId)
    {
        var foundGameGenre = await _gameRepository.GetGameGenreByIdAsync(id);
        if (!foundGameGenre.Succeeded || foundGameGenre.Result is null)
        {
            return await Result.FailAsync(ErrorMessageConstants.GameGenres.NotFound);
        }

        var deleteGenre = await _gameRepository.DeleteGameGenreAsync(id);
        if (!deleteGenre.Succeeded)
        {
            var tshootId = await _auditRepository.CreateTroubleshootLog(_dateTime, AuditTableName.TshootGameGenres, foundGameGenre.Result.Id, requestUserId, new Dictionary<string, string>
            {
                {"Detail", "Failed to delete game genre"},
                {"Error", deleteGenre.ErrorMessage}
            });
            return await Result<Guid>.FailAsync([ErrorMessageConstants.Generic.ContactAdmin, ErrorMessageConstants.Audit.AuditRecordId(tshootId.Data)]);
        }
        
        await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.GameGenres, foundGameGenre.Result.Id, requestUserId, DatabaseActionType.Delete,
            foundGameGenre.Result);

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

    public async Task<IResult<IEnumerable<GameUpdateSlim>>> GetAllGameUpdatesAsync()
    {
        var request = await _gameRepository.GetAllGameUpdatesAsync();
        if (!request.Succeeded)
        {
            return await Result<IEnumerable<GameUpdateSlim>>.FailAsync(request.ErrorMessage);
        }

        return await Result<IEnumerable<GameUpdateSlim>>.SuccessAsync(request.Result?.ToSlims() ?? []);
    }

    public async Task<IResult<IEnumerable<GameUpdateSlim>>> GetAllGameUpdatesPaginatedAsync(int pageNumber, int pageSize)
    {
        var request = await _gameRepository.GetAllGameUpdatesPaginatedAsync(pageNumber, pageSize);
        if (!request.Succeeded)
        {
            return await Result<IEnumerable<GameUpdateSlim>>.FailAsync(request.ErrorMessage);
        }

        return await Result<IEnumerable<GameUpdateSlim>>.SuccessAsync(request.Result?.ToSlims() ?? []);
    }

    public async Task<IResult<int>> GetGameUpdatesCountAsync()
    {
        var request = await _gameRepository.GetGameUpdatesCountAsync();
        if (!request.Succeeded)
        {
            return await Result<int>.FailAsync(request.ErrorMessage);
        }

        return await Result<int>.SuccessAsync(request.Result);
    }

    public async Task<IResult<GameUpdateSlim?>> GetGameUpdateByIdAsync(Guid id)
    {
        var request = await _gameRepository.GetGameUpdateByIdAsync(id);
        if (!request.Succeeded)
        {
            return await Result<GameUpdateSlim?>.FailAsync(request.ErrorMessage);
        }

        return await Result<GameUpdateSlim?>.SuccessAsync(request.Result?.ToSlim());
    }

    public async Task<IResult<IEnumerable<GameUpdateSlim>>> GetGameUpdatesByGameId(Guid id)
    {
        var request = await _gameRepository.GetGameUpdatesByGameId(id);
        if (!request.Succeeded)
        {
            return await Result<IEnumerable<GameUpdateSlim>>.FailAsync(request.ErrorMessage);
        }

        return await Result<IEnumerable<GameUpdateSlim>>.SuccessAsync(request.Result?.ToSlims() ?? []);
    }

    public async Task<IResult<Guid>> CreateGameUpdateAsync(GameUpdateCreate createObject)
    {
        var foundGame = await _gameRepository.GetByIdAsync(createObject.GameId);
        if (foundGame.Result is null)
        {
            return await Result<Guid>.FailAsync(ErrorMessageConstants.Games.NotFound);
        }
        
        var request = await _gameRepository.CreateGameUpdateAsync(createObject);
        if (!request.Succeeded)
            return await Result<Guid>.FailAsync(request.ErrorMessage);

        _eventService.TriggerGameVersionUpdate("CreateGameUpdateAsync", new GameVersionUpdatedEvent
        {
            GameId = foundGame.Result.Id,
            AppId = foundGame.Result.SteamToolId,
            AppName = foundGame.Result.FriendlyName,
            VersionBuild = createObject.BuildVersion
        });

        return await Result<Guid>.SuccessAsync(request.Result);
    }

    public async Task<IResult> DeleteGameUpdateAsync(Guid id)
    {
        var request = await _gameRepository.DeleteGameUpdateAsync(id);
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteGameUpdatesForGameIdAsync(Guid id)
    {
        var request = await _gameRepository.DeleteGameUpdatesForGameIdAsync(id);
        if (!request.Succeeded)
            return await Result.FailAsync(request.ErrorMessage);

        return await Result.SuccessAsync();
    }

    public async Task<IResult<IEnumerable<GameUpdateSlim>>> SearchGameUpdateAsync(string searchText)
    {
        var request = await _gameRepository.SearchGameUpdateAsync(searchText);
        if (!request.Succeeded)
        {
            return await Result<IEnumerable<GameUpdateSlim>>.FailAsync(request.ErrorMessage);
        }

        return await Result<IEnumerable<GameUpdateSlim>>.SuccessAsync(request.Result?.ToSlims() ?? []);
    }

    public async Task<IResult<IEnumerable<GameUpdateSlim>>> SearchGameUpdatePaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        var request = await _gameRepository.SearchGameUpdatePaginatedAsync(searchText, pageNumber, pageSize);
        if (!request.Succeeded)
        {
            return await Result<IEnumerable<GameUpdateSlim>>.FailAsync(request.ErrorMessage);
        }

        return await Result<IEnumerable<GameUpdateSlim>>.SuccessAsync(request.Result?.ToSlims() ?? []);
    }
}