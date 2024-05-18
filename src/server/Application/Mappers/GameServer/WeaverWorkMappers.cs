using Application.Models.Events;
using Application.Models.GameServer.WeaverWork;
using Domain.DatabaseEntities.GameServer;
using Domain.Enums.GameServer;

namespace Application.Mappers.GameServer;

public static class WeaverWorkMappers
{
    public static WeaverWorkSlim ToSlim(this WeaverWorkDb weaverWorkDb)
    {
        return new WeaverWorkSlim
        {
            Id = weaverWorkDb.Id,
            HostId = weaverWorkDb.HostId,
            TargetType = weaverWorkDb.TargetType,
            Status = weaverWorkDb.Status,
            WorkData = weaverWorkDb.WorkData,
            CreatedBy = weaverWorkDb.CreatedBy,
            CreatedOn = weaverWorkDb.CreatedOn,
            LastModifiedBy = weaverWorkDb.LastModifiedBy,
            LastModifiedOn = weaverWorkDb.LastModifiedOn
        };
    }
    
    public static IEnumerable<WeaverWorkSlim> ToSlims(this IEnumerable<WeaverWorkDb> weaverWorkDbs)
    {
        return weaverWorkDbs.Select(ToSlim);
    }
    
    public static WeaverWorkUpdate ToUpdate(this WeaverWorkDb weaverWorkDb)
    {
        return new WeaverWorkUpdate
        {
            Id = weaverWorkDb.Id,
            HostId = weaverWorkDb.HostId,
            TargetType = weaverWorkDb.TargetType,
            Status = weaverWorkDb.Status,
            WorkData = weaverWorkDb.WorkData,
            CreatedBy = weaverWorkDb.CreatedBy,
            CreatedOn = weaverWorkDb.CreatedOn,
            LastModifiedBy = weaverWorkDb.LastModifiedBy,
            LastModifiedOn = weaverWorkDb.LastModifiedOn
        };
    }

    public static WeaverWorkStatusEvent ToEvent(this WeaverWorkUpdate update)
    {
        return new WeaverWorkStatusEvent
        {
            Id = update.Id,
            HostId = update.HostId,
            TargetType = update.TargetType ?? WeaverWorkTarget.StatusUpdate,
            Status = update.Status ?? WeaverWorkState.InProgress
        };
    }
}