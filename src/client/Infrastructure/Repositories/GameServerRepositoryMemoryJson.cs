using Application.Constants;
using Application.Models;
using Application.Models.GameServer;
using Application.Repositories;
using Application.Services;
using Domain.Contracts;
using Domain.Models.GameServer;

namespace Infrastructure.Repositories;

public class GameServerRepositoryMemoryJson : IGameServerRepository
{
    private readonly ISerializerService _serializerService;
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTimeService;

    public GameServerRepositoryMemoryJson(ISerializerService serializerService, ILogger logger, IDateTimeService dateTimeService)
    {
        _serializerService = serializerService;
        _logger = logger;
        _dateTimeService = dateTimeService;

        Task.Run(LoadAsync);
    }
    
    private static List<GameServerLocal> _gameServers = [];

    public async Task<IResult> CreateAsync(GameServerLocal gameServer)
    {
        var existingGameserver = _gameServers.FirstOrDefault(x => x.Id == gameServer.Id);
        if (existingGameserver is not null)
            return await Result.FailAsync($"Gameserver with Id [{existingGameserver.Id}] already exists: {existingGameserver.ServerName}");

        gameServer.LastStateUpdate = _dateTimeService.NowDatabaseTime;
        _gameServers.Add(gameServer);
        _logger.Information("Successfully created gameserver: [{GameserverID}]{GameserverName}", gameServer.Id, gameServer.ServerName);
        await SaveAsync();
        return await Result.SuccessAsync();
    }

    public async Task<IResult> UpdateAsync(GameServerLocalUpdate gameServerUpdate)
    {
        var existingGameserver = _gameServers.FirstOrDefault(x => x.Id == gameServerUpdate.Id);
        if (existingGameserver is null)
            return await Result.FailAsync($"Gameserver with Id [{gameServerUpdate.Id}] doesn't exist");
        
        existingGameserver.SteamName = gameServerUpdate.SteamName ?? existingGameserver.SteamName;
        existingGameserver.SteamGameId = gameServerUpdate.SteamGameId ?? existingGameserver.SteamGameId;
        existingGameserver.SteamToolId = gameServerUpdate.SteamToolId ?? existingGameserver.SteamToolId;
        existingGameserver.ServerName = gameServerUpdate.ServerName ?? existingGameserver.ServerName;
        existingGameserver.Password = gameServerUpdate.Password ?? existingGameserver.Password;
        existingGameserver.PasswordRcon = gameServerUpdate.PasswordRcon ?? existingGameserver.PasswordRcon;
        existingGameserver.PasswordAdmin = gameServerUpdate.PasswordAdmin ?? existingGameserver.PasswordAdmin;
        existingGameserver.ServerVersion = gameServerUpdate.ServerVersion ?? existingGameserver.ServerVersion;
        existingGameserver.IpAddress = gameServerUpdate.IpAddress ?? existingGameserver.IpAddress;
        existingGameserver.ExtHostname = gameServerUpdate.ExtHostname ?? existingGameserver.ExtHostname;
        existingGameserver.PortGame = gameServerUpdate.PortGame ?? existingGameserver.PortGame;
        existingGameserver.PortQuery = gameServerUpdate.PortQuery ?? existingGameserver.PortQuery;
        existingGameserver.PortRcon = gameServerUpdate.PortRcon ?? existingGameserver.PortRcon;
        existingGameserver.Modded = gameServerUpdate.Modded ?? existingGameserver.Modded;
        existingGameserver.ManualRootUrl = gameServerUpdate.ManualRootUrl ?? existingGameserver.ManualRootUrl;
        existingGameserver.ServerProcessName = gameServerUpdate.ServerProcessName ?? existingGameserver.ServerProcessName;
        existingGameserver.LastStateUpdate = _dateTimeService.NowDatabaseTime;
        existingGameserver.ServerState = gameServerUpdate.ServerState ?? existingGameserver.ServerState;
        existingGameserver.Source = gameServerUpdate.Source ?? existingGameserver.Source;
        existingGameserver.ModList = gameServerUpdate.ModList ?? existingGameserver.ModList;
        existingGameserver.Resources = gameServerUpdate.Resources ?? existingGameserver.Resources;
        existingGameserver.UpdatesWaiting = gameServerUpdate.UpdatesWaiting ?? existingGameserver.UpdatesWaiting;

        _logger.Debug("Successfully updated gameserver: [{GameserverID}]{GameserverName}", existingGameserver.Id, existingGameserver.ServerName);
        return await Result.SuccessAsync();
    }

    public async Task<IResult> DeleteAsync(Guid id)
    {
        var existingGameserver = _gameServers.FirstOrDefault(x => x.Id == id);
        if (existingGameserver is null)
            return await Result.FailAsync($"Gameserver with Id [{id}] doesn't exist");
        
        _gameServers.Remove(existingGameserver);
        _logger.Information("Successfully deleted gameserver: [{GameserverID}]{GameserverName}", existingGameserver.Id, existingGameserver.ServerName);
        await SaveAsync();
        return await Result.SuccessAsync();
    }

    public async Task<IResult> SaveAsync()
    {
        try
        {
            var serializedGameserverState = _serializerService.SerializeJson(_gameServers);
            await File.WriteAllTextAsync(SerializerConstants.GameServerStatePath, serializedGameserverState);
            _logger.Information("Serialized gameserver state file: {FilePath}", SerializerConstants.GameServerStatePath);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred serializing gameserver repository to json: {Error}", ex.Message);
            return await Result.FailAsync($"Failure occurred serializing gameserver repository to json: {ex.Message}");
        }
    }

    public async Task<IResult> LoadAsync()
    {
        if (!File.Exists(SerializerConstants.GameServerStatePath))
        {
            _logger.Debug("Gameserver state file doesn't exist, creating... [{FilePath}]", SerializerConstants.GameServerStatePath);
            await SaveAsync();
        }

        try
        {
            var gameServerState = await File.ReadAllTextAsync(SerializerConstants.GameServerStatePath);
            _gameServers = _serializerService.DeserializeJson<List<GameServerLocal>>(gameServerState);
            _logger.Information("Deserialized gameserver state: {FilePath}", SerializerConstants.GameServerStatePath);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred deserializing gameserver repository from json: {Error}", ex.Message);
            return await Result.FailAsync($"Failure occurred deserializing gameserver repository from json: {ex.Message}");
        }
    }

    public async Task<IResult<GameServerLocal?>> GetByIdAsync(Guid id)
    {
        var existingGameserver = _gameServers.FirstOrDefault(x => x.Id == id);
        if (existingGameserver is null)
            return await Result<GameServerLocal?>.FailAsync($"Gameserver with Id [{id}] doesn't exist");

        return await Result<GameServerLocal?>.SuccessAsync(existingGameserver);
    }

    public async Task<IResult<List<GameServerLocal>>> GetAll()
    {
        return await Result<List<GameServerLocal>>.SuccessAsync(_gameServers);
    }
}