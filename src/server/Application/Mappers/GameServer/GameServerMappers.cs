using Application.Models.GameServer.GameServer;
using Domain.DatabaseEntities.GameServer;

namespace Application.Mappers.GameServer;

public static class GameServerMappers
{
    public static GameServerSlim ToSlim(this GameServerDb gameServerDb)
    {
        return new GameServerSlim
        {
            Id = gameServerDb.Id,
            OwnerId = gameServerDb.OwnerId,
            HostId = gameServerDb.HostId,
            GameId = gameServerDb.GameId,
            GameProfileId = gameServerDb.GameProfileId,
            ServerName = gameServerDb.ServerName,
            Password = gameServerDb.Password,
            PasswordRcon = gameServerDb.PasswordRcon,
            PasswordAdmin = gameServerDb.PasswordAdmin,
            PublicIp = gameServerDb.PublicIp,
            PrivateIp = gameServerDb.PrivateIp,
            ExternalHostname = gameServerDb.ExternalHostname,
            PortGame = gameServerDb.PortGame,
            PortQuery = gameServerDb.PortQuery,
            PortRcon = gameServerDb.PortRcon,
            Modded = gameServerDb.Modded,
            Private = gameServerDb.Private,
            ServerState = gameServerDb.ServerState,
            CreatedBy = gameServerDb.CreatedBy,
            CreatedOn = gameServerDb.CreatedOn,
            LastModifiedBy = gameServerDb.LastModifiedBy,
            LastModifiedOn = gameServerDb.LastModifiedOn,
            IsDeleted = gameServerDb.IsDeleted,
            DeletedOn = gameServerDb.DeletedOn
        };
    }
    
    public static IEnumerable<GameServerSlim> ToSlims(this IEnumerable<GameServerDb> gameServerDbs)
    {
        return gameServerDbs.Select(ToSlim);
    }

    public static GameServerUpdate ToUpdate(this GameServerDb gameServerDb)
    {
        return new GameServerUpdate
        {
            Id = gameServerDb.Id,
            OwnerId = gameServerDb.OwnerId,
            HostId = gameServerDb.HostId,
            GameId = gameServerDb.GameId,
            GameProfileId = gameServerDb.GameProfileId,
            ServerName = gameServerDb.ServerName,
            Password = gameServerDb.Password,
            PasswordRcon = gameServerDb.PasswordRcon,
            PasswordAdmin = gameServerDb.PasswordAdmin,
            PublicIp = gameServerDb.PublicIp,
            PrivateIp = gameServerDb.PrivateIp,
            ExternalHostname = gameServerDb.ExternalHostname,
            PortGame = gameServerDb.PortGame,
            PortQuery = gameServerDb.PortQuery,
            PortRcon = gameServerDb.PortRcon,
            Modded = gameServerDb.Modded,
            Private = gameServerDb.Private,
            ServerState = gameServerDb.ServerState,
            CreatedBy = gameServerDb.CreatedBy,
            CreatedOn = gameServerDb.CreatedOn,
            LastModifiedBy = gameServerDb.LastModifiedBy,
            LastModifiedOn = gameServerDb.LastModifiedOn,
            IsDeleted = gameServerDb.IsDeleted,
            DeletedOn = gameServerDb.DeletedOn
        };
    }

    public static GameServerFull ToFull(this GameServerDb gameServerDb)
    {
        return new GameServerFull
        {
            Id = gameServerDb.Id,
            OwnerId = gameServerDb.OwnerId,
            HostId = gameServerDb.HostId,
            GameId = gameServerDb.GameId,
            GameProfileId = gameServerDb.GameProfileId,
            ServerName = gameServerDb.ServerName,
            Password = gameServerDb.Password,
            PasswordRcon = gameServerDb.PasswordRcon,
            PasswordAdmin = gameServerDb.PasswordAdmin,
            PublicIp = gameServerDb.PublicIp,
            PrivateIp = gameServerDb.PrivateIp,
            ExternalHostname = gameServerDb.ExternalHostname,
            PortGame = gameServerDb.PortGame,
            PortQuery = gameServerDb.PortQuery,
            PortRcon = gameServerDb.PortRcon,
            Modded = gameServerDb.Modded,
            Private = gameServerDb.Private,
            ServerState = gameServerDb.ServerState,
            CreatedBy = gameServerDb.CreatedBy,
            CreatedOn = gameServerDb.CreatedOn,
            LastModifiedBy = gameServerDb.LastModifiedBy,
            LastModifiedOn = gameServerDb.LastModifiedOn,
            IsDeleted = gameServerDb.IsDeleted,
            DeletedOn = gameServerDb.DeletedOn
        };
    }
}