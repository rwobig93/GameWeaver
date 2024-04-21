using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.WeaverWork;
using Application.Repositories.GameServer;
using Domain.Enums.GameServer;
using Domain.Models.Database;
using MemoryPack;

namespace Application.Helpers.GameServer;

public static class WeaverWorkHelpers
{
    public static Task<DatabaseActionResult<int>> SendWeaverWork<T>(this IHostRepository repository, WeaverWorkTarget workTarget,
        Guid hostId, T workData, Guid modifyingUserId, DateTime createdOn)
    {
        return repository.CreateWeaverWorkAsync(new WeaverWorkCreate
        {
            HostId = hostId,
            TargetType = workTarget,
            Status = WeaverWorkState.WaitingToBePickedUp,
            WorkData = MemoryPackSerializer.Serialize(workData),
            CreatedBy = modifyingUserId,
            CreatedOn = createdOn,
            LastModifiedBy = null,
            LastModifiedOn = null
        });
    }
    
    public static Task<DatabaseActionResult<int>> SendGameserverStateUpdate(this IHostRepository repository, Guid hostId,
        GameServerToHost gameServer, Guid modifyingUserId, DateTime createdOn)
    {
        return repository.SendWeaverWork(WeaverWorkTarget.GameServerStateUpdate, hostId, gameServer, modifyingUserId, createdOn);
    }
}