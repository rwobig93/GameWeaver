using Application.Models.Events;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.WeaverWork;
using Application.Requests.GameServer.GameServer;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.GameServer;

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
            ParentGameProfileId = gameServerDb.ParentGameProfileId,
            ServerBuildVersion = gameServerDb.ServerBuildVersion,
            ServerName = gameServerDb.ServerName,
            Password = gameServerDb.Password,
            PasswordRcon = gameServerDb.PasswordRcon,
            PasswordAdmin = gameServerDb.PasswordAdmin,
            PublicIp = gameServerDb.PublicIp,
            PrivateIp = gameServerDb.PrivateIp,
            ExternalHostname = gameServerDb.ExternalHostname,
            PortGame = gameServerDb.PortGame,
            PortPeer = gameServerDb.PortPeer,
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
            ParentGameProfileId = gameServerDb.ParentGameProfileId,
            ServerBuildVersion = gameServerDb.ServerBuildVersion,
            ServerName = gameServerDb.ServerName,
            Password = gameServerDb.Password,
            PasswordRcon = gameServerDb.PasswordRcon,
            PasswordAdmin = gameServerDb.PasswordAdmin,
            PublicIp = gameServerDb.PublicIp,
            PrivateIp = gameServerDb.PrivateIp,
            ExternalHostname = gameServerDb.ExternalHostname,
            PortGame = gameServerDb.PortGame,
            PortPeer = gameServerDb.PortPeer,
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
            ParentGameProfileId = gameServerDb.ParentGameProfileId,
            ServerBuildVersion = gameServerDb.ServerBuildVersion,
            ServerName = gameServerDb.ServerName,
            Password = gameServerDb.Password,
            PasswordRcon = gameServerDb.PasswordRcon,
            PasswordAdmin = gameServerDb.PasswordAdmin,
            PublicIp = gameServerDb.PublicIp,
            PrivateIp = gameServerDb.PrivateIp,
            ExternalHostname = gameServerDb.ExternalHostname,
            PortGame = gameServerDb.PortGame,
            PortPeer = gameServerDb.PortPeer,
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

    public static GameServerToHost ToHost(this GameServerDb gameServerDb)
    {
        return new GameServerToHost
        {
            Id = gameServerDb.Id,
            GameId = gameServerDb.GameId,
            SteamName = gameServerDb.ServerName,
            SteamGameId = 0,
            SteamToolId = 0,
            ServerName = gameServerDb.ServerName,
            Password = gameServerDb.Password,
            PasswordRcon = gameServerDb.PasswordRcon,
            PasswordAdmin = gameServerDb.PasswordAdmin,
            ServerVersion = gameServerDb.ServerBuildVersion,
            IpAddress = gameServerDb.PrivateIp,
            ExtHostname = gameServerDb.PublicIp,
            PortGame = gameServerDb.PortGame,
            PortPeer = gameServerDb.PortPeer,
            PortQuery = gameServerDb.PortQuery,
            PortRcon = gameServerDb.PortRcon,
            Modded = gameServerDb.Modded,
            ManualRootUrl = "",
            ServerProcessName = "",
            ServerState = ConnectivityState.Unknown,
            Source = GameSource.Steam,
            ModList = [],
            Resources = []
        };
    }

    public static GameServerStatusEvent ToStatusEvent(this GameServerDb gameServer)
    {
        return new GameServerStatusEvent
        {
            Id = gameServer.Id,
            ServerName = gameServer.ServerName,
            BuildVersionUpdated = false,
            ServerState = gameServer.ServerState
        };
    }

    public static GameServerStatusEvent ToStatusEvent(this GameServerStateUpdate update)
    {
        return new GameServerStatusEvent
        {
            Id = update.Id,
            ServerName = "",
            BuildVersionUpdated = update.BuildVersionUpdated,
            ServerState = update.ServerState
        };
    }

    public static GameServerCreate ToCreate(this GameServerCreateRequest request)
    {
        return new GameServerCreate
        {
            OwnerId = request.OwnerId,
            HostId = request.HostId,
            GameId = request.GameId,
            GameProfileId = Guid.Empty,
            ParentGameProfileId = request.ParentGameProfileId,
            ServerName = request.Name,
            Password = request.Password,
            PasswordRcon = request.PasswordRcon,
            PasswordAdmin = request.PasswordAdmin,
            ExternalHostname = request.ExternalUrl,
            PortGame = request.PortGame,
            PortPeer = request.PortPeer,
            PortQuery = request.PortQuery,
            PortRcon = request.PortRcon,
            Modded = request.Modded,
            Private = request.Private
        };
    }

    public static GameServerUpdate ToUpdate(this GameServerUpdateRequest request)
    {
        return new GameServerUpdate
        {
            Id = request.Id,
            OwnerId = request.OwnerId,
            ParentGameProfileId = request.ParentGameProfileId,
            ServerBuildVersion = request.ServerBuildVersion,
            ServerName = request.ServerName,
            Password = request.Password,
            PasswordRcon = request.PasswordRcon,
            PasswordAdmin = request.PasswordAdmin,
            PortGame = request.PortGame,
            PortPeer = request.PortPeer,
            PortQuery = request.PortQuery,
            PortRcon = request.PortRcon,
            Modded = request.Modded,
            Private = request.Private
        };
    }
}