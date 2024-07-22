using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameServer;
using Application.Services.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class GameView : ComponentBase
{
    [Parameter] public Guid GameId { get; set; } = Guid.Empty;

    [Inject] public IGameService GameService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;

    private bool _validIdProvided = true;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private GameSlim _game = new() { Id = Guid.Empty };
    private bool _editMode;
    private string _editButtonText = "Enable Edit Mode";
    private List<GameServerSlim> _runningGameservers = [];

    private bool _canEditGame;
    private bool _canViewGameServers;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await GetClientTimezone();
                await GetViewingGame();
                await GetPermissions();
                StateHasChanged();
            }
        }
        catch
        {
            StateHasChanged();
        }
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
        {
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }

    private async Task GetViewingGame()
    {
        var response = await GameService.GetByIdAsync(GameId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (response.Data is null)
        {
            Snackbar.Add(ErrorMessageConstants.Games.NotFound);
            _validIdProvided = false;
            return;
        }

        _game = response.Data;
        
        if (_game.Id == Guid.Empty)
        {
            _validIdProvided = false;
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canEditGame = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Game.Update);
        _canViewGameServers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get);
    }
    
    private async Task Save()
    {
        if (!_canEditGame) return;
        
        // TODO: Add save logic
        
        ToggleEditMode();
        await GetViewingGame();
        Snackbar.Add("Game successfully updated!", Severity.Success);
        StateHasChanged();
    }

    private void ToggleEditMode()
    {
        _editMode = !_editMode;

        _editButtonText = _editMode ? "Disable Edit Mode" : "Enable Edit Mode";
    }

    private void GoBack()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.Games.ViewAll);
    }

    private async Task ViewGameOnSteam()
    {
        var urlOpened = await WebClientService.OpenExternalUrl(_game.UrlSteamStorePage);
        if (!urlOpened.Succeeded)
        {
            urlOpened.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
        }
    }
}