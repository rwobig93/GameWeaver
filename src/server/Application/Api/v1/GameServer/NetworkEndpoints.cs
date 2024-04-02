using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.Network;
using Application.Services.GameServer;
using Domain.Contracts;
using Domain.Enums.GameServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.v1.GameServer;

/// <summary>
/// Endpoints dealing with network operations
/// </summary>
public static class NetworkEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsNetwork(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRouteConstants.GameServer.Network.GameserverConnectable, GameserverConnectable).ApiVersionOne();
        app.MapPost(ApiRouteConstants.GameServer.Network.IsPortOpen, IsPortOpen).ApiVersionOne();
    }

    /// <summary>
    /// Get whether a game server is connectable
    /// </summary>
    /// <param name="check">Required parameters to check for game server connectivity</param>
    /// <param name="networkService"></param>
    /// <returns>Boolean indicating a game server is connectable</returns>
    [Authorize(PermissionConstants.Network.GameserverConnectable)]
    private static async Task<IResult<bool>> GameserverConnectable(GameServerConnectivityCheck check, INetworkService networkService)
    {
        try
        {
            return await networkService.IsGameServerConnectableAsync(check);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }

    /// <summary>
    /// Get whether a port is open or gives a valid response
    /// </summary>
    /// <param name="ipAddress">Ip or hostname to send a check to</param>
    /// <param name="port">Port to send a check to</param>
    /// <param name="protocol">0=TCP, 1=UDP</param>
    /// <param name="timeoutMilliseconds">Timeout in milliseconds</param>
    /// <param name="networkService"></param>
    /// <returns>Boolean indicating the port is open / responds</returns>
    [Authorize(PermissionConstants.Network.IsPortOpen)]
    private static async Task<IResult<bool>> IsPortOpen([FromBody]string ipAddress, [FromBody]int port, [FromBody]NetworkProtocol protocol, [FromBody]int timeoutMilliseconds,
        INetworkService networkService)
    {
        try
        {
            return await networkService.IsPortOpenAsync(ipAddress, port, protocol, timeoutMilliseconds);
        }
        catch (Exception ex)
        {
            return await Result<bool>.FailAsync(ex.Message);
        }
    }
}