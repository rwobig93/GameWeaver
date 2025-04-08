using Application.Models.GameServer.GameServer;

namespace GameWeaver.Components.GameServer;

public partial class GameServerListWidget : ComponentBase
{
    [Parameter] public IEnumerable<GameServerSlim> GameServers { get; set; } = null!;
    [Parameter] public IEnumerable<Guid> ViewableGameServers { get; set; } = [];
    [Parameter] public string Title { get; set; } = "Game Servers";


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await UpdateViewableGameServers();
        }
    }

    private async Task UpdateViewableGameServers()
    {
        if (!ViewableGameServers.Any())  // If no viewable gameservers were provided we'll assume we can view all of them
        {
            ViewableGameServers = GameServers.Select(x => x.Id);
            StateHasChanged();
        }

        await Task.CompletedTask;
    }

    private void ViewGameServer(Guid id)
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.GameServers.ViewId(id));
    }
}