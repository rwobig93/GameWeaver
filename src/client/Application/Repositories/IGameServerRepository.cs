using Application.Models;
using Domain.Contracts;
using Domain.Models.GameServer;

namespace Application.Repositories;

public interface IGameServerRepository
{
    Task<IResult> CreateAsync(GameServerLocal gameServer);
    Task<IResult> UpdateAsync(GameServerLocalUpdate gameServerUpdate);
    Task<IResult> DeleteAsync(Guid id);
    Task<IResult> SaveAsync();
    Task<IResult> LoadAsync();
    Task<IResult<GameServerLocal?>> GetByIdAsync(Guid id);
    Task<IResult<List<GameServerLocal>>> GetAll();
}