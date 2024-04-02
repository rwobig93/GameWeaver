using Application.Constants.Identity;
using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.GameServer.GameServer;
using Application.Services.GameServer;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace Application.Api.v1.GameServer;

/// <summary>
/// Endpoints dealing with game server operations
/// </summary>
public static class GameServerEndpoints
{
    /// <summary>
    /// Registers the API endpoints
    /// </summary>
    /// <param name="app">Running application</param>
    public static void MapEndpointsGameserver(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRouteConstants.GameServer.Host.CreateRegistration, GetAll).ApiVersionOne();
    }

    /// <summary>
    /// Get all game server hosts
    /// </summary>
    /// <param name="gameServerService"></param>
    /// <returns>List of game server hosts</returns>
    [Authorize(PermissionConstants.Gameserver.GetAll)]
    private static async Task<IResult<IEnumerable<GameServerSlim>>> GetAll(IGameServerService gameServerService)
    {
        try
        {
            return await gameServerService.GetAllAsync();
        }
        catch (Exception ex)
        {
            return await Result<IEnumerable<GameServerSlim>>.FailAsync(ex.Message);
        }
    }
}