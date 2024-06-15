using Application.Models.GameServer.Network;
using Domain.Contracts;
using Domain.Enums.GameServer;

namespace Application.Services.GameServer;

public interface INetworkService
{
    Task<IResult<bool>> IsGameServerConnectableAsync(GameServerConnectivityCheck check);
    Task<IResult<bool>> IsPortOpenAsync(string ipOrHost, int port, NetworkProtocol protocol, int timeoutMilliseconds);
}