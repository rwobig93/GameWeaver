using Application.Models.GameServer.GameProfile;
using Application.Requests.GameServer.GameProfile;
using Domain.DatabaseEntities.GameServer;

namespace Application.Mappers.GameServer;

public static class GameProfileMappers
{
    public static GameProfileSlim ToSlim(this GameProfileDb gameProfileDb)
    {
        return new GameProfileSlim
        {
            Id = gameProfileDb.Id,
            FriendlyName = gameProfileDb.FriendlyName,
            OwnerId = gameProfileDb.OwnerId,
            GameId = gameProfileDb.GameId,
            ServerProcessName = gameProfileDb.ServerProcessName,
            CreatedBy = gameProfileDb.CreatedBy,
            CreatedOn = gameProfileDb.CreatedOn,
            LastModifiedBy = gameProfileDb.LastModifiedBy,
            LastModifiedOn = gameProfileDb.LastModifiedOn,
            IsDeleted = gameProfileDb.IsDeleted,
            DeletedOn = gameProfileDb.DeletedOn
        };
    }
    
    public static IEnumerable<GameProfileSlim> ToSlims(this IEnumerable<GameProfileDb> gameProfileDbs)
    {
        return gameProfileDbs.Select(ToSlim);
    }
    
    public static GameProfileUpdate ToUpdate(this GameProfileDb gameProfileDb)
    {
        return new GameProfileUpdate
        {
            Id = gameProfileDb.Id,
            FriendlyName = gameProfileDb.FriendlyName,
            OwnerId = gameProfileDb.OwnerId,
            GameId = gameProfileDb.GameId,
            ServerProcessName = gameProfileDb.ServerProcessName,
            CreatedBy = gameProfileDb.CreatedBy,
            CreatedOn = gameProfileDb.CreatedOn,
            LastModifiedBy = gameProfileDb.LastModifiedBy,
            LastModifiedOn = gameProfileDb.LastModifiedOn,
            IsDeleted = gameProfileDb.IsDeleted,
            DeletedOn = gameProfileDb.DeletedOn
        };
    }

    public static GameProfileCreate ToCreate(this GameProfileCreateRequest request)
    {
        return new GameProfileCreate
        {
            FriendlyName = request.Name,
            OwnerId = request.OwnerId,
            GameId = request.GameId
        };
    }

    public static GameProfileUpdate ToUpdate(this GameProfileUpdateRequest request)
    {
        return new GameProfileUpdate
        {
            Id = request.Id,
            FriendlyName = request.Name,
            OwnerId = request.OwnerId
        };
    }
}