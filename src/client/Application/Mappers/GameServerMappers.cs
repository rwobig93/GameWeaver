using Application.Models;
using Domain.Models.ControlServer;
using Domain.Models.GameServer;

namespace Application.Mappers;

public static class GameServerMappers
{
    public static GameServerLocal ToLocal(this GameServerToHost gameServerToHost)
    {
        return new GameServerLocal
        {
            Id = gameServerToHost.Id,
            SteamName = gameServerToHost.SteamName,
            SteamGameId = gameServerToHost.SteamGameId,
            SteamToolId = gameServerToHost.SteamToolId,
            ServerName = gameServerToHost.ServerName,
            Password = gameServerToHost.Password,
            PasswordRcon = gameServerToHost.PasswordRcon,
            PasswordAdmin = gameServerToHost.PasswordAdmin,
            ServerVersion = gameServerToHost.ServerVersion,
            IpAddress = gameServerToHost.IpAddress,
            ExtHostname = gameServerToHost.ExtHostname,
            PortGame = gameServerToHost.PortGame,
            PortQuery = gameServerToHost.PortQuery,
            PortRcon = gameServerToHost.PortRcon,
            Modded = gameServerToHost.Modded,
            ManualRootUrl = gameServerToHost.ManualRootUrl,
            ServerProcessName = gameServerToHost.ServerProcessName,
            LastStateUpdate = DateTime.Now,
            ServerState = gameServerToHost.ServerState,
            Source = gameServerToHost.Source,
            ModList = gameServerToHost.ModList,
            Resources = gameServerToHost.Resources,
            UpdatesWaiting = []
        };
    }

    public static GameServerLocalUpdate ToUpdate(this GameServerToHost gameServerToHost)
    {
        return new GameServerLocalUpdate
        {
            Id = gameServerToHost.Id,
            SteamName = gameServerToHost.SteamName,
            SteamGameId = gameServerToHost.SteamGameId,
            SteamToolId = gameServerToHost.SteamToolId,
            ServerName = gameServerToHost.ServerName,
            Password = gameServerToHost.Password,
            PasswordRcon = gameServerToHost.PasswordRcon,
            PasswordAdmin = gameServerToHost.PasswordAdmin,
            ServerVersion = gameServerToHost.ServerVersion,
            IpAddress = gameServerToHost.IpAddress,
            ExtHostname = gameServerToHost.ExtHostname,
            PortGame = gameServerToHost.PortGame,
            PortQuery = gameServerToHost.PortQuery,
            PortRcon = gameServerToHost.PortRcon,
            Modded = gameServerToHost.Modded,
            ManualRootUrl = gameServerToHost.ManualRootUrl,
            ServerProcessName = gameServerToHost.ServerProcessName,
            LastStateUpdate = DateTime.Now,
            ServerState = gameServerToHost.ServerState,
            Source = gameServerToHost.Source,
            ModList = gameServerToHost.ModList,
            Resources = gameServerToHost.Resources,
            UpdatesWaiting = []
        };
    }

    public static GameServerLocalUpdate ToUpdate(this GameServerLocal gameServerLocal)
    {
        return new GameServerLocalUpdate
        {
            Id = gameServerLocal.Id,
            SteamName = gameServerLocal.SteamName,
            SteamGameId = gameServerLocal.SteamGameId,
            SteamToolId = gameServerLocal.SteamToolId,
            ServerName = gameServerLocal.ServerName,
            Password = gameServerLocal.Password,
            PasswordRcon = gameServerLocal.PasswordRcon,
            PasswordAdmin = gameServerLocal.PasswordAdmin,
            ServerVersion = gameServerLocal.ServerVersion,
            IpAddress = gameServerLocal.IpAddress,
            ExtHostname = gameServerLocal.ExtHostname,
            PortGame = gameServerLocal.PortGame,
            PortQuery = gameServerLocal.PortQuery,
            PortRcon = gameServerLocal.PortRcon,
            Modded = gameServerLocal.Modded,
            ManualRootUrl = gameServerLocal.ManualRootUrl,
            ServerProcessName = gameServerLocal.ServerProcessName,
            LastStateUpdate = DateTime.Now,
            ServerState = gameServerLocal.ServerState,
            Source = gameServerLocal.Source,
            ModList = gameServerLocal.ModList,
            Resources = gameServerLocal.Resources,
            UpdatesWaiting = []
        };
    }
}