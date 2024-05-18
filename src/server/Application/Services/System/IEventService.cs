using Application.Models.Events;

namespace Application.Services.System;

public interface IEventService
{
    public event EventHandler<GameServerStatusEvent>? GameServerStatusChanged;
    public event EventHandler<WeaverWorkStatusEvent>? WeaverWorkStatusChanged;
    
    void TriggerGameServerStatus(string source, GameServerStatusEvent args);
    void TriggerWeaverWorkStatus(string source, WeaverWorkStatusEvent args);
}