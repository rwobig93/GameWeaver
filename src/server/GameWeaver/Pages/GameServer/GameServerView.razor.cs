using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.ConfigResourceTreeItem;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Requests.GameServer.LocalResource;
using Application.Services.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class GameServerView : ComponentBase
{
    [Parameter] public Guid GameServerId { get; set; } = Guid.Empty;

    [Inject] public IGameServerService GameServerService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;

    private bool _validIdProvided = true;
    private Guid _loggedInUserId = Guid.Empty;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private GameServerSlim _gameServer = new() { Id = Guid.Empty };
    private List<LocalResourceSlim> _localResources = [];
    private bool _editMode;
    private string _editButtonText = "Enable Edit Mode";
    private string _searchText = string.Empty;
    private readonly List<ConfigurationItemSlim> _updatedConfigItems = [];

    private bool _canEditGameServer;
    private bool _canDeleteGameServer;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await GetPermissions();
                await GetClientTimezone();
                await GetViewingGameServer();
                await GetGameServerResources();
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

    private async Task GetViewingGameServer()
    {
        var response = await GameServerService.GetByIdAsync(GameServerId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (response.Data is null)
        {
            Snackbar.Add(ErrorMessageConstants.GameServers.NotFound);
            _validIdProvided = false;
            return;
        }

        _gameServer = response.Data;
        
        if (_gameServer.Id == Guid.Empty)
        {
            _validIdProvided = false;
            StateHasChanged();
        }
    }

    private async Task GetGameServerResources()
    {
        if (!_validIdProvided)
        {
            return;
        }
        
        var response = await GameServerService.GetLocalResourcesForGameServerIdAsync(_gameServer.Id);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        _localResources = response.Data.ToList();
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _loggedInUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        _canEditGameServer = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Update);
        _canDeleteGameServer = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Delete);
    }
    
    private async Task Save()
    {
        if (!_canEditGameServer)
        {
            return;
        }
        
        var response = await GameServerService.UpdateAsync(_gameServer.ToUpdate(), _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var configItem in _updatedConfigItems)
        {
            var configResponse = await GameServerService.UpdateConfigurationItemAsync(configItem.ToUpdate(_loggedInUserId), _loggedInUserId);
            if (configResponse.Succeeded) continue;
            
            configResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (_updatedConfigItems.Count > 0)
        {
            var hostUpdateResponse = await GameServerService.UpdateAllLocalResourcesOnGameServerAsync(_gameServer.Id, _loggedInUserId);
            if (!hostUpdateResponse.Succeeded)
            {
                hostUpdateResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }
        }
        
        ToggleEditMode();
        await GetViewingGameServer();
        await GetGameServerResources();
        
        Snackbar.Add("Gameserver successfully updated!", Severity.Success);
        StateHasChanged();
    }

    private void ToggleEditMode()
    {
        _editMode = !_editMode;

        _editButtonText = _editMode ? "Disable Edit Mode" : "Enable Edit Mode";
    }

    private void GoBack()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.GameServers.ViewAll);
    }

    private async Task DeleteGameServer()
    {
        if (!_canDeleteGameServer)
        {
            Snackbar.Add(ErrorMessageConstants.Permissions.PermissionError, Severity.Error);
            return;
        }
        
        var dialogParameters = new DialogParameters()
        {
            {"Title", "Are you sure you want to delete this gameserver?"},
            {"Content", $"Server Name: {_gameServer.ServerName}"}
        };
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };

        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Delete Gameserver", dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var response = await GameServerService.DeleteAsync(_gameServer.Id, _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        Snackbar.Add("Gameserver successfully deleted!", Severity.Success);
        GoBack();
    }
    
    private bool ConfigShouldBeShown(ConfigurationItemSlim item)
    {
        var shouldBeShown = item.FriendlyName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                            item.Key.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                            item.Value.Contains(_searchText, StringComparison.OrdinalIgnoreCase);
        
        return shouldBeShown;
    }

    private void ConfigUpdated(ConfigurationItemSlim item)
    {
        var matchingConfigItem = _updatedConfigItems.FirstOrDefault(x => x.Id == item.Id);
        if (matchingConfigItem is null)
        {
            _updatedConfigItems.Add(item);
            return;
        }
        
        matchingConfigItem.Value = item.Value;
    }
}   