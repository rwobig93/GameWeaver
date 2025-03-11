using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameServer;
using Application.Services.GameServer;

namespace GameWeaver.Components.GameServer;

public partial class GameServerWidget : ComponentBase
{
    [Parameter] public GameServerSlim GameServer { get; set; } = new();
    [Parameter] public int WidthPx { get; set; } = 400;
    [Parameter] public int HeightPx { get; set; } = 100;
    
    [Inject] public IGameService GameService { get; init; } = null!;

    private GameSlim _game = new() { Id = Guid.Empty, FriendlyName = "Unknown" };
    
    
    private string Width => $"{WidthPx}px";
    private string Height => $"{HeightPx}px";
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetGame();
        }
    }

    private async Task GetGame()
    {
        var response = await GameService.GetByIdAsync(GameServer.GameId);
        if (response.Data is not null)
        {
            _game = response.Data;
            StateHasChanged();
        }
    }

    private void ViewServer()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.GameServers.ViewId(GameServer.Id));
    }
}