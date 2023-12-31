﻿using Application.Helpers.Lifecycle;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.Developers;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameGenre;
using Application.Models.GameServer.Publishers;
using Application.Repositories.GameServer;
using Application.Repositories.Lifecycle;
using Application.Services.Database;
using Application.Services.System;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.Database;
using Domain.Enums.Lifecycle;
using Domain.Models.Database;
using Infrastructure.Database.MsSql.GameServer;
using Infrastructure.Database.MsSql.Shared;

namespace Infrastructure.Repositories.MsSql.GameServer;

public class GameRepositoryMsSql : IGameRepository
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTime;
    private readonly IAuditTrailsRepository _auditRepository;

    public GameRepositoryMsSql(ISqlDataService database, ILogger logger, IDateTimeService dateTime, IAuditTrailsRepository auditRepository)
    {
        _database = database;
        _logger = logger;
        _dateTime = dateTime;
        _auditRepository = auditRepository;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameDb>>> GetAllAsync()
    {
        DatabaseActionResult<IEnumerable<GameDb>> actionReturn = new();

        try
        {
            var allGames = await _database.LoadData<GameDb, dynamic>(GamesTableMsSql.GetAll, new { });
            actionReturn.Succeed(allGames);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameDb>>> GetAllPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<GameDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var allGames = await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            actionReturn.Succeed(allGames);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {GamesTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameDb>> GetByIdAsync(Guid id)
    {
        DatabaseActionResult<GameDb> actionReturn = new();

        try
        {
            var foundGame = (await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundGame!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameDb>>> GetBySteamNameAsync(string steamName)
    {
        DatabaseActionResult<IEnumerable<GameDb>> actionReturn = new();

        try
        {
            var foundGames = await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetBySteamName, new {SteamName = steamName});
            actionReturn.Succeed(foundGames);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetBySteamName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameDb>> GetByFriendlyNameAsync(string friendlyName)
    {
        DatabaseActionResult<GameDb> actionReturn = new();

        try
        {
            var foundGame = (await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetByFriendlyName, new {FriendlyName = friendlyName})).FirstOrDefault();
            actionReturn.Succeed(foundGame!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetByFriendlyName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameDb>> GetBySteamGameIdAsync(int id)
    {
        DatabaseActionResult<GameDb> actionReturn = new();

        try
        {
            var foundGame = (await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetBySteamGameId, new {SteamGameId = id})).FirstOrDefault();
            actionReturn.Succeed(foundGame!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetBySteamGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameDb>> GetBySteamToolIdAsync(int id)
    {
        DatabaseActionResult<GameDb> actionReturn = new();

        try
        {
            var foundGame = (await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetBySteamToolId, new {SteamToolId = id})).FirstOrDefault();
            actionReturn.Succeed(foundGame!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.GetBySteamToolId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateAsync(GameCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            createObject.CreatedOn = _dateTime.NowDatabaseTime;

            var createdId = await _database.SaveDataReturnId(GamesTableMsSql.Insert, createObject);

            var foundHost = await GetByIdAsync(createdId);

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Games, foundHost.Result!.Id,
                createObject.CreatedBy, DatabaseActionType.Create, null, foundHost.Result!.ToSlim());

            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> UpdateAsync(GameUpdate updateObject)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var beforeObject = (await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetById, new {updateObject.Id})).FirstOrDefault();
            
            await _database.SaveData(GamesTableMsSql.Update, updateObject);
            
            var afterObject = (await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.GetById, new {updateObject.Id})).FirstOrDefault();

            var updateDiff = AuditHelpers.GetAuditDiff(beforeObject, afterObject);

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Games, beforeObject!.Id,
                updateObject.LastModifiedBy.GetFromNullable(), DatabaseActionType.Update, updateDiff.Before, updateDiff.After);
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.Update.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteAsync(Guid id, Guid modifyingUserId)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var foundGame = await GetByIdAsync(id);
            if (!foundGame.Succeeded || foundGame.Result is null)
                throw new Exception(foundGame.ErrorMessage);
            var gameUpdate = foundGame.Result.ToUpdate();
            
            // Update user w/ a property that is modified so we get the last updated on/by for the deleting user
            gameUpdate.LastModifiedBy = modifyingUserId;
            await UpdateAsync(gameUpdate);
            await _database.SaveData(GamesTableMsSql.Delete, 
                new { Id = id, DeletedOn = _dateTime.NowDatabaseTime });

            await _auditRepository.CreateAuditTrail(_dateTime, AuditTableName.Games, id,
                gameUpdate.LastModifiedBy.GetFromNullable(), DatabaseActionType.Delete, gameUpdate);
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameDb>>> SearchAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<GameDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.Search, new { SearchTerm = searchText });
            
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameDb>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<GameDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var searchResults = await _database.LoadData<GameDb, dynamic>(
                GamesTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GamesTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> GetAllDevelopersAsync()
    {
        DatabaseActionResult<IEnumerable<DeveloperDb>> actionReturn = new();

        try
        {
            var allDevelopers = await _database.LoadData<DeveloperDb, dynamic>(DevelopersTableMsSql.GetAll, new { });
            actionReturn.Succeed(allDevelopers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> GetAllDevelopersPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<DeveloperDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var allDevelopers = await _database.LoadData<DeveloperDb, dynamic>(
                DevelopersTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            actionReturn.Succeed(allDevelopers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetDevelopersCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {DevelopersTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<DeveloperDb>> GetDeveloperByIdAsync(Guid id)
    {
        DatabaseActionResult<DeveloperDb> actionReturn = new();

        try
        {
            var foundDeveloper = (await _database.LoadData<DeveloperDb, dynamic>(
                DevelopersTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundDeveloper!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<DeveloperDb>> GetDeveloperByNameAsync(string name)
    {
        DatabaseActionResult<DeveloperDb> actionReturn = new();

        try
        {
            var foundDeveloper = (await _database.LoadData<DeveloperDb, dynamic>(
                DevelopersTableMsSql.GetByName, new {Name = name})).FirstOrDefault();
            actionReturn.Succeed(foundDeveloper!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.GetByName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> GetDevelopersByGameIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<DeveloperDb>> actionReturn = new();

        try
        {
            var foundDeveloper = await _database.LoadData<DeveloperDb, dynamic>(
                DevelopersTableMsSql.GetByGameId, new {Id = id});
            actionReturn.Succeed(foundDeveloper);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.GetByGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateDeveloperAsync(DeveloperCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(DevelopersTableMsSql.Insert, createObject);
            
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteDeveloperAsync(Guid id)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var foundDeveloper = await GetDeveloperByIdAsync(id);
            if (!foundDeveloper.Succeeded || foundDeveloper.Result is null)
                throw new Exception(foundDeveloper.ErrorMessage);

            await _database.SaveData(DevelopersTableMsSql.Delete, new { Id = id });
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> SearchDevelopersAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<DeveloperDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<DeveloperDb, dynamic>(
                DevelopersTableMsSql.Search, new { SearchTerm = searchText });
            
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<DeveloperDb>>> SearchDevelopersPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<DeveloperDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var searchResults = await _database.LoadData<DeveloperDb, dynamic>(
                DevelopersTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, DevelopersTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<PublisherDb>>> GetAllPublishersAsync()
    {
        DatabaseActionResult<IEnumerable<PublisherDb>> actionReturn = new();

        try
        {
            var allPublishers = await _database.LoadData<PublisherDb, dynamic>(PublishersTableMsSql.GetAll, new { });
            actionReturn.Succeed(allPublishers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<PublisherDb>>> GetAllPublishersPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<PublisherDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var allPublishers = await _database.LoadData<PublisherDb, dynamic>(
                PublishersTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            actionReturn.Succeed(allPublishers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetPublishersCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {PublishersTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PublisherDb>> GetPublisherByIdAsync(Guid id)
    {
        DatabaseActionResult<PublisherDb> actionReturn = new();

        try
        {
            var foundPublisher = (await _database.LoadData<PublisherDb, dynamic>(
                PublishersTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundPublisher!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<PublisherDb>> GetPublisherByNameAsync(string name)
    {
        DatabaseActionResult<PublisherDb> actionReturn = new();

        try
        {
            var foundPublisher = (await _database.LoadData<PublisherDb, dynamic>(
                PublishersTableMsSql.GetByName, new {Name = name})).FirstOrDefault();
            actionReturn.Succeed(foundPublisher!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.GetByName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<PublisherDb>>> GetPublishersByGameIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<PublisherDb>> actionReturn = new();

        try
        {
            var foundPublishers = await _database.LoadData<PublisherDb, dynamic>(
                PublishersTableMsSql.GetByGameId, new {Id = id});
            actionReturn.Succeed(foundPublishers);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.GetByGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreatePublisherAsync(PublisherCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(PublishersTableMsSql.Insert, createObject);
            
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeletePublisherAsync(Guid id)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var foundPublisher = await GetPublisherByIdAsync(id);
            if (!foundPublisher.Succeeded || foundPublisher.Result is null)
                throw new Exception(foundPublisher.ErrorMessage);

            await _database.SaveData(PublishersTableMsSql.Delete, new { Id = id });
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<PublisherDb>>> SearchPublishersAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<PublisherDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<PublisherDb, dynamic>(
                PublishersTableMsSql.Search, new { SearchTerm = searchText });
            
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<PublisherDb>>> SearchPublishersPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<PublisherDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var searchResults = await _database.LoadData<PublisherDb, dynamic>(
                PublishersTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, PublishersTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> GetAllGameGenresAsync()
    {
        DatabaseActionResult<IEnumerable<GameGenreDb>> actionReturn = new();

        try
        {
            var allGenres = await _database.LoadData<GameGenreDb, dynamic>(GameGenreTableMsSql.GetAll, new { });
            actionReturn.Succeed(allGenres);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.GetAll.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> GetAllGameGenresPaginatedAsync(int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<GameGenreDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var allGenres = await _database.LoadData<GameGenreDb, dynamic>(
                GameGenreTableMsSql.GetAllPaginated, new {Offset =  offset, PageSize = pageSize});
            actionReturn.Succeed(allGenres);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.GetAllPaginated.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<int>> GetGameGenresCountAsync()
    {
        DatabaseActionResult<int> actionReturn = new();

        try
        {
            var rowCount = (await _database.LoadData<int, dynamic>(
                GeneralTableMsSql.GetRowCount, new {GameGenreTableMsSql.Table.TableName})).FirstOrDefault();
            actionReturn.Succeed(rowCount);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GeneralTableMsSql.GetRowCount.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameGenreDb>> GetGameGenreByIdAsync(Guid id)
    {
        DatabaseActionResult<GameGenreDb> actionReturn = new();

        try
        {
            var foundGenre = (await _database.LoadData<GameGenreDb, dynamic>(
                GameGenreTableMsSql.GetById, new {Id = id})).FirstOrDefault();
            actionReturn.Succeed(foundGenre!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.GetById.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<GameGenreDb>> GetGameGenreByNameAsync(string name)
    {
        DatabaseActionResult<GameGenreDb> actionReturn = new();

        try
        {
            var foundGenre = (await _database.LoadData<GameGenreDb, dynamic>(
                GameGenreTableMsSql.GetByName, new {Name = name})).FirstOrDefault();
            actionReturn.Succeed(foundGenre!);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.GetByName.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> GetGameGenresByGameIdAsync(Guid id)
    {
        DatabaseActionResult<IEnumerable<GameGenreDb>> actionReturn = new();

        try
        {
            var foundGenres = await _database.LoadData<GameGenreDb, dynamic>(
                GameGenreTableMsSql.GetByGameId, new {Id = id});
            actionReturn.Succeed(foundGenres);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.GetByGameId.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<Guid>> CreateGameGenreAsync(GameGenreCreate createObject)
    {
        DatabaseActionResult<Guid> actionReturn = new();

        try
        {
            var createdId = await _database.SaveDataReturnId(GameGenreTableMsSql.Insert, createObject);
            
            actionReturn.Succeed(createdId);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.Insert.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult> DeleteGameGenreAsync(Guid id)
    {
        DatabaseActionResult actionReturn = new();

        try
        {
            var foundGenre = await GetGameGenreByIdAsync(id);
            if (!foundGenre.Succeeded || foundGenre.Result is null)
                throw new Exception(foundGenre.ErrorMessage);

            await _database.SaveData(GameGenreTableMsSql.Delete, new { Id = id });
            
            actionReturn.Succeed();
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.Delete.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> SearchGameGenresAsync(string searchText)
    {
        DatabaseActionResult<IEnumerable<GameGenreDb>> actionReturn = new();

        try
        {
            var searchResults = await _database.LoadData<GameGenreDb, dynamic>(
                GameGenreTableMsSql.Search, new { SearchTerm = searchText });
            
            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.Search.Path, ex.Message);
        }

        return actionReturn;
    }

    public async Task<DatabaseActionResult<IEnumerable<GameGenreDb>>> SearchGameGenresPaginatedAsync(string searchText, int pageNumber, int pageSize)
    {
        DatabaseActionResult<IEnumerable<GameGenreDb>> actionReturn = new();

        try
        {
            var offset = MathHelpers.GetPaginatedOffset(pageNumber, pageSize);
            var searchResults = await _database.LoadData<GameGenreDb, dynamic>(
                GameGenreTableMsSql.SearchPaginated, new { SearchTerm = searchText, Offset = offset, PageSize = pageSize });

            actionReturn.Succeed(searchResults);
        }
        catch (Exception ex)
        {
            actionReturn.FailLog(_logger, GameGenreTableMsSql.SearchPaginated.Path, ex.Message);
        }

        return actionReturn;
    }
}