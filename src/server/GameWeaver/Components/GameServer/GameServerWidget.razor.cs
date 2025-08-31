using Application.Helpers.GameServer;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameServer;
using Application.Services.GameServer;

namespace GameWeaver.Components.GameServer;

public partial class GameServerWidget : ComponentBase
{
    private string _cssBorderBase = "rounded-lg justify-center align-center mud-text-align-center";
    private string _cssBorderStatus = " border-status-default";
    private string _cssTextStatus = "";

    private GameSlim _game = new() {Id = Guid.Empty, FriendlyName = "Unknown"};
    [Parameter] public GameServerSlim GameServer { get; set; } = new();
    [Parameter] public int WidthPx { get; set; } = 400;
    [Parameter] public int HeightPx { get; set; } = 100;
    [Parameter] public bool GamerMode { get; set; }

    [Inject] public IGameService GameService { get; init; } = null!;


    private string Width => $"{WidthPx}px";
    private string Height => $"{HeightPx}px";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetGame();
        }
    }

    private void UpdateBorderStatus()
    {
        if (GameServer.Id == Guid.Empty)
        {
            _cssBorderStatus = " border-status-default";
            return;
        }

        if (GameServer.ServerState.IsRunning())
        {
            if (GamerMode)
            {
                _cssBorderStatus = " border-rainbow-glow";
                _cssTextStatus = "rainbow-text";
                return;
            }

            _cssBorderStatus = " border-status-success";
            return;
        }

        _cssBorderStatus = " border-status-error";
    }

    private async Task GetGame()
    {
        var response = await GameService.GetByIdAsync(GameServer.GameId);
        if (response.Data is not null)
        {
            _game = response.Data;
            UpdateBorderStatus();
            StateHasChanged();
        }
    }

    private void ViewServer()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.GameServers.ViewId(GameServer.Id));
    }
}