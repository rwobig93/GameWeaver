using Application.Models.GameServer.WeaverWork;
using Domain.DatabaseEntities.GameServer;

namespace Application.Mappers.GameServer;

public static class WeaverWorkMappers
{
    public static WeaverWorkSlim ToSlim(this WeaverWorkDb weaverWorkDb)
    {
        return new WeaverWorkSlim
        {
            Id = weaverWorkDb.Id,
            HostId = weaverWorkDb.HostId,
            GameServerId = weaverWorkDb.GameServerId,
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
            GameServerId = weaverWorkDb.GameServerId,
            TargetType = weaverWorkDb.TargetType,
            Status = weaverWorkDb.Status,
            WorkData = weaverWorkDb.WorkData,
            CreatedBy = weaverWorkDb.CreatedBy,
            CreatedOn = weaverWorkDb.CreatedOn,
            LastModifiedBy = weaverWorkDb.LastModifiedBy,
            LastModifiedOn = weaverWorkDb.LastModifiedOn
        };
    }
}