using Application.Requests.Host;
using Domain.Enums;
using Domain.Models.ControlServer;
using MemoryPack;
using WeaverService.Workers;

namespace WeaverService.Helpers;

public static class WorkHelper
{
    public static void SendStatusUpdate(this WeaverWork work, WeaverWorkState status, IEnumerable<string>? messages = null)
    {
        messages ??= new List<string>();
        
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.StatusUpdate,
            Status = status,
            WorkData = MemoryPackSerializer.Serialize(messages),
            AttemptCount = 0
        });
    }
    
    public static void SendStatusUpdate(this WeaverWork work, WeaverWorkState status, string message)
    {
        work.SendStatusUpdate(status, new List<string> { message });
    }
    
    public static void SendGameServerUpdate(this WeaverWork work, WeaverWorkState status, object workData)
    {
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.GameServerStateUpdate,
            Status = status,
            WorkData = MemoryPackSerializer.Serialize(workData),
            AttemptCount = 0
        });
    }
    
    public static void SendHostDetailUpdate(this WeaverWork work, WeaverWorkState status, object workData)
    {
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = WeaverWorkTarget.HostDetail,
            Status = status,
            WorkData = MemoryPackSerializer.Serialize(workData),
            AttemptCount = 0
        });
    }
}