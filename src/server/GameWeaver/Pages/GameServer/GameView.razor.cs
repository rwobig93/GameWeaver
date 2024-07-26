using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameServer;
using Application.Models.Integrations;
using Application.Services.GameServer;
using Application.Services.Integrations;
using Domain.Enums.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class GameView : ComponentBase
{
    [Parameter] public Guid GameId { get; set; } = Guid.Empty;

    [Inject] public IGameService GameService { get; set; } = null!;
    [Inject] public IGameServerService GameServerService { get; set; } = null!;
    [Inject] public IFileStorageRecordService FileStorageService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;

    private bool _validIdProvided = true;
    private Guid _loggedInUserId = Guid.Empty;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private GameSlim _game = new() { Id = Guid.Empty };
    private bool _editMode;
    private string _editButtonText = "Enable Edit Mode";
    private List<GameServerSlim> _runningGameservers = [];
    private List<FileStorageRecordSlim> _manualVersionFiles = [];

    private bool _canEditGame;
    private bool _canViewGameServers;
    private bool _canViewGameFiles;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await GetPermissions();
                await GetClientTimezone();
                await GetViewingGame();
                await GetGameVersionFiles();
                await GetGameServers();
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

    private async Task GetGameServers()
    {
        if (!_canViewGameServers)
        {
            return;
        }

        _runningGameservers = [];
        var response = await GameServerService.GetByGameIdAsync(_game.Id);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _runningGameservers = response.Data.ToList();
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _loggedInUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        _canEditGame = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Game.Update);
        _canViewGameServers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get);
        _canViewGameFiles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.GameVersions.Get);
    }
    
    private async Task Save()
    {
        if (!_canEditGame)
        {
            return;
        }
        
        var response = await GameService.UpdateAsync(_game.ToUpdate(), _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
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

    private async Task ViewGameVersionFiles()
    {
        // TODO: Write open modal logic
        await Task.CompletedTask;
    }

    private async Task GetGameVersionFiles()
    {
        if (!_canViewGameFiles && _game.SourceType == GameSource.Manual)
        {
            return;
        }

        _manualVersionFiles = [];
        var response = await FileStorageService.GetByLinkedIdAsync(_game.Id);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _manualVersionFiles = response.Data.ToList();
    }
}