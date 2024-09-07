using Application.Models.GameServer.GameServer;

namespace GameWeaver.Components.GameServer;

public partial class GameServerWidget : ComponentBase
{
    [Parameter] public GameServerSlim GameServer { get; set; } = new();
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.CompletedTask;
        }
    }

    private void ViewServer()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.GameServers.ViewId(GameServer.Id));
    }
}