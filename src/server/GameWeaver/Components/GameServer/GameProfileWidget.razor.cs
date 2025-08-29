using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.GameServer;
using Application.Services.GameServer;
using Application.Services.Lifecycle;

namespace GameWeaver.Components.GameServer;

public partial class GameProfileWidget : ComponentBase
{
    [Parameter] public GameProfileSlim GameProfile { get; set; } = new();
    [Parameter] public int WidthPx { get; set; } = 400;
    [Parameter] public int HeightPx { get; set; } = 100;
    [Parameter] public bool GamerMode { get; set; }

    [Inject] public IGameService GameService { get; init; } = null!;
    [Inject] public IGameServerService GameServerService { get; init; } = null!;
    [Inject] public IRunningServerState ServerState { get; init; } = null!;

    private GameSlim _game = new() { Id = Guid.Empty, FriendlyName = "Unknown" };
    private GameServerSlim _directGameServer = new() {Id = Guid.Empty, ServerName = "None"};
    public readonly string _cssBorderBase = "rounded-lg justify-center align-center mud-text-align-center";
    private string _cssBorderStatus = " border-status-default";
    private string _cssTextStatus = "";
    private int _serverUsages;


    private string Width => $"{WidthPx}px";
    private string Height => $"{HeightPx}px";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetGame();
            await GetProfileUsage();
            UpdateBorderStatus();
            StateHasChanged();
        }
    }

    private async Task GetGame()
    {
        var response = await GameService.GetByIdAsync(GameProfile.GameId);
        if (!response.Succeeded || response.Data is null)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _game = response.Data;
    }

    private async Task GetProfileUsage()
    {
        if (GameProfile.Id == _game.DefaultGameProfileId)  // The default game profile is on every server for that game, we'll check game usage instead
        {
            var gameResponse = await GameServerService.GetByGameIdAsync(_game.Id, ServerState.SystemUserId);
            if (!gameResponse.Succeeded)
            {
                gameResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
            _serverUsages = gameResponse.Data.Count();
            return;
        }

        // Get count of all servers inheriting this profile as a 'parent' profile
        var parentProfileResponse = await GameServerService.GetByParentGameProfileIdAsync(GameProfile.Id, ServerState.SystemUserId);
        if (!parentProfileResponse.Succeeded)
        {
            parentProfileResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _serverUsages = parentProfileResponse.Data.Count();

        // If this profile is a server profile, we'll indicate such and increase server usage since otherwise we only look at profile inheritance
        var directServerResponse = await GameServerService.GetByGameProfileIdAsync(GameProfile.Id, ServerState.SystemUserId);
        if (directServerResponse is {Succeeded: true, Data: not null})
        {
            _directGameServer = directServerResponse.Data;
            _serverUsages++;
        }
    }

    private void UpdateBorderStatus()
    {
        if (GameProfile.Id == Guid.Empty)
        {
            _cssBorderStatus = " border-status-default";
            return;
        }

        if (_serverUsages > 0)
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

        // Profile is a game default but doesn't have any active servers, we'll use a generic border color instead of the error color
        if (GameProfile.Id == _game.DefaultGameProfileId)
        {
            _cssBorderStatus = " border-status-default";
            return;
        }

        _cssBorderStatus = " border-status-error";
    }


    private Color GetAssignmentColor()
    {
        if (_serverUsages > 0)
        {
            return Color.Success;
        }

        // Profile is a game default but doesn't have any active servers, we'll use a generic color instead of the error color
        if (GameProfile.Id == _game.DefaultGameProfileId)
        {
            return Color.Secondary;
        }

        return Color.Error;
    }

    private void ViewProfile()
    {
        // The default game profile shouldn't be separately modified, we'll go to the game page instead
        if (GameProfile.Id == _game.DefaultGameProfileId)
        {
            NavManager.NavigateTo(AppRouteConstants.GameServer.Games.ViewId(GameProfile.GameId));
            return;
        }
        // The game profile for a server shouldn't be separately modified, we'll go to the game server page instead
        if (_directGameServer.Id != Guid.Empty)
        {
            NavManager.NavigateTo(AppRouteConstants.GameServer.GameServers.ViewId(_directGameServer.Id));
            return;
        }

        NavManager.NavigateTo(AppRouteConstants.GameServer.GameProfiles.ViewId(GameProfile.Id));
    }
}