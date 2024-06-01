using Application.Models.Events;

namespace Application.Services.System;

public interface IEventService
{
    public event EventHandler<GameServerStatusEvent>? GameServerStatusChanged;
    public event EventHandler<WeaverWorkStatusEvent>? WeaverWorkStatusChanged;
    public event EventHandler<GameVersionUpdatedEvent>? GameVersionUpdated; 
    
    void TriggerGameServerStatus(string source, GameServerStatusEvent args);
    void TriggerWeaverWorkStatus(string source, WeaverWorkStatusEvent args);
    void TriggerGameVersionUpdate(string source, GameVersionUpdatedEvent args);
}