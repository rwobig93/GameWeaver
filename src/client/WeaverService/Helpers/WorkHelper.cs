using Application.Requests.Host;
using Domain.Enums;
using Domain.Models.ControlServer;
using MemoryPack;
using WeaverService.Workers;

namespace WeaverService.Helpers;

public static class WorkHelper
{
    public static void SendWeaverWorkUpdate<T>(this WeaverWork work, WeaverWorkTarget workTarget, WeaverWorkState workState, T workData)
    {
        ControlServerWorker.AddWeaverWorkUpdate(new WeaverWorkUpdateRequest
        {
            Id = work.Id,
            TargetType = workTarget,
            Status = workState,
            WorkData = MemoryPackSerializer.Serialize(workData),
            AttemptCount = 0
        });
    }
    
    public static void SendStatusUpdate(this WeaverWork work, WeaverWorkState status, IEnumerable<string>? messages = null)
    {
        messages ??= new List<string>();
        
        work.SendWeaverWorkUpdate(WeaverWorkTarget.StatusUpdate, status, messages);
    }
    
    public static void SendStatusUpdate(this WeaverWork work, WeaverWorkState status, string message)
    {
        work.SendStatusUpdate(status, new List<string> { message });
    }
    
    public static void SendGameServerUpdate(this WeaverWork work, WeaverWorkState status, GameServerStateUpdate workData)
    {
        work.SendWeaverWorkUpdate(WeaverWorkTarget.GameServerStateUpdate, status, workData);
    }
    
    public static void SendHostDetailUpdate(this WeaverWork work, WeaverWorkState status, HostDetailRequest? workData)
    {
        work.SendWeaverWorkUpdate(WeaverWorkTarget.HostDetail, status, workData);
    }
}