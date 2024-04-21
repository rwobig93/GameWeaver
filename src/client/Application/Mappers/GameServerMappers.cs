using Domain.Contracts;
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
}