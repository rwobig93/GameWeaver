using Application.Models.Events;
using Application.Services.System;

namespace Infrastructure.Services.System;

public class EventService : IEventService
{
    public event EventHandler<GameServerStatusEvent>? GameServerStatusChanged;
    public event EventHandler<WeaverWorkStatusEvent>? WeaverWorkStatusChanged;
    public event EventHandler<GameVersionUpdatedEvent>? GameVersionUpdated; 
    
    public void TriggerGameServerStatus(string source, GameServerStatusEvent args)
    {
        GameServerStatusChanged?.Invoke(source, args);
    }

    public void TriggerWeaverWorkStatus(string source, WeaverWorkStatusEvent args)
    {
        WeaverWorkStatusChanged?.Invoke(source, args);
    }

    public void TriggerGameVersionUpdate(string source, GameVersionUpdatedEvent args)
    {
        GameVersionUpdated?.Invoke(source, args);
    }
}