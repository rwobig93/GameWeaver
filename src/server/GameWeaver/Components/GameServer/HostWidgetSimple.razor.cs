using Application.Helpers.GameServer;
using Application.Models.GameServer.Host;
using Application.Services.GameServer;
using Application.Services.Lifecycle;

namespace GameWeaver.Components.GameServer;

public partial class HostWidgetSimple : ComponentBase
{
    [Parameter] public HostSlim Host { get; set; } = null!;
    [Parameter] public int WidthPx { get; set; } = 400;
    [Parameter] public int HeightPx { get; set; } = 100;
    [Parameter] public bool GamerMode { get; set; }

    [Inject] private IGameServerService GameServerService { get; init; } = null!;
    [Inject] private IRunningServerState ServerState { get; init; } = null!;

    private string Width => $"{WidthPx}px";
    private string Height => $"{HeightPx}px";
    private string _imageUrl = "/images/gameserver/host-default-vertical.jpg";
    private string _cssBorderBase = "rounded-lg justify-center align-center mud-text-align-center";
    private string _cssBorderStatus = " border-status-default";
    private string _cssTextStatus = "";
    private int _hostGameServers;


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            UpdateBorderStatus();
            await GetHostGameServerCount();
        }
    }

    private void UpdateBorderStatus()
    {
        if (Host.Id == Guid.Empty)
        {
            _cssBorderStatus = " border-status-default";
            StateHasChanged();
            return;
        }

        if (Host.CurrentState.IsRunning())
        {
            if (GamerMode)
            {
                _cssBorderStatus = " border-rainbow-glow";
                _cssTextStatus = "rainbow-text";
                StateHasChanged();
                return;
            }

            _cssBorderStatus = " border-status-success";
            StateHasChanged();
            return;
        }

        _cssBorderStatus = " border-status-error";
        StateHasChanged();
    }

    private async Task GetHostGameServerCount()
    {
        var hostGameServersRequest = await GameServerService.GetByHostIdAsync(Host.Id, ServerState.SystemUserId);
        if (!hostGameServersRequest.Succeeded)
        {
            hostGameServersRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _hostGameServers = hostGameServersRequest.Data.Count();
        StateHasChanged();
    }

    private void ViewHost()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.Hosts.ViewId(Host.Id));
    }
}