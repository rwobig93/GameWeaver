using Application.Constants.Web;
using Application.SignalR.Models;
using Microsoft.AspNetCore.SignalR;

namespace Application.SignalR.Hubs;

public class GameServerHub : Hub
{
    public async Task GameServerStateChange(GameServerStatusSignal signal)
    {
        // TODO: Figure out how to listen for Groups then we can send to Group(serverId) instead of All
        await Clients.All.SendCoreAsync(SignalRConstants.GameServer.StatusUpdate, [signal]);
    }
}