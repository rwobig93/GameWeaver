using Application.Models.GameServer.GameServer;

namespace GameWeaver.Components.GameServer;

public partial class GameServerWidget : ComponentBase
{
    [Parameter] public GameServerSlim GameServer { get; set; } = new();
    [Parameter] public int WidthPx { get; set; } = 350;
    [Parameter] public int HeightPx { get; set; } = 150;
    
    
    private string Width => $"{WidthPx}px";
    private string Height => $"{HeightPx}px";
    
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