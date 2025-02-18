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
using Domain.Enums.Identity;
using GameWeaver.Components.GameServer;

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
    private readonly List<ConfigurationItemSlim> _createdConfigItems = [];
    private readonly List<ConfigurationItemSlim> _updatedConfigItems = [];
    private readonly List<ConfigurationItemSlim> _deletedConfigItems = [];

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
        if (!_canEditGameServer)
        {
            _canEditGameServer = await AuthorizationService.UserHasPermission(currentUser, 
                PermissionConstants.GameServer.Gameserver.Dynamic(_gameServer.Id, DynamicPermissionLevel.Configure));
        }
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

        foreach (var configItem in _createdConfigItems)
        {
            var createConfigResponse = await GameServerService.CreateConfigurationItemAsync(configItem.ToCreate(), _loggedInUserId);
            if (createConfigResponse.Succeeded) continue;
            
            createConfigResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var configItem in _updatedConfigItems)
        {
            // Config item comes from inherited profile resource, create server profile resource for the config item
            if (configItem.LocalResourceId == Guid.Empty)
            {
                var matchingLocalResource = _localResources.First(x => x.ConfigSets.Any(c => c.Id == configItem.Id));
                var createResourceResponse = await GameServerService.CreateLocalResourceAsync(matchingLocalResource.ToCreateRequest(), _loggedInUserId);
                if (!createResourceResponse.Succeeded)
                {
                    createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return;
                }
                
                configItem.LocalResourceId = createResourceResponse.Data;
            }
            
            var updateConfigResponse = await GameServerService.UpdateConfigurationItemAsync(configItem.ToUpdate(), _loggedInUserId);
            if (updateConfigResponse.Succeeded) continue;
            
            updateConfigResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var configItem in _deletedConfigItems)
        {
            // Config item comes from inherited profile resource, create server profile resource for the config item to be ignored
            if (configItem.LocalResourceId == Guid.Empty)
            {
                var matchingLocalResource = _localResources.First(x => x.ConfigSets.Any(c => c.Id == configItem.Id));
                var createResourceResponse = await GameServerService.CreateLocalResourceAsync(matchingLocalResource.ToCreateRequest(), _loggedInUserId);
                if (!createResourceResponse.Succeeded)
                {
                    createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return;
                }
                
                configItem.LocalResourceId = createResourceResponse.Data;
                configItem.Ignore = true;
                
                var ignoreCreateResponse = await GameServerService.CreateConfigurationItemAsync(configItem.ToCreate(), _loggedInUserId);
                if (!ignoreCreateResponse.Succeeded)
                {
                    ignoreCreateResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return;
                }
            }

            var deleteConfigResponse = await GameServerService.DeleteConfigurationItemAsync(configItem.Id, _loggedInUserId);
            if (deleteConfigResponse.Succeeded) continue;
            
            deleteConfigResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
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

    private async Task ConfigAdd(LocalResourceSlim localResource)
    {
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<ConfigAddDialog>("Add Config Item", new DialogParameters(), dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var newConfigItem = (ConfigurationItemSlim) dialogResult.Data;
        
        _createdConfigItems.Add(newConfigItem);
        localResource.ConfigSets = localResource.ConfigSets.ToList().Prepend(newConfigItem);
    }
    
    private void ConfigUpdated(ConfigurationItemSlim item)
    {
        var matchingNewConfig = _createdConfigItems.FirstOrDefault(x => x.Id == item.Id);
        if (matchingNewConfig is not null)
        {
            matchingNewConfig.Value = item.Value;
            return;
        }
        
        var matchingUpdateConfig = _updatedConfigItems.FirstOrDefault(x => x.Id == item.Id);
        if (matchingUpdateConfig is null)
        {
            _updatedConfigItems.Add(item);
            return;
        }
        
        matchingUpdateConfig.Value = item.Value;
    }

    private void ConfigDeleted(ConfigurationItemSlim item)
    {
        if (_createdConfigItems.Contains(item))
        {
            _createdConfigItems.Remove(item);
            return;
        }
        
        // Go through each resource config set and remove the targeted config item
        foreach (var resource in _localResources)
        {
            // TODO: Only remove from resource if it is not a parent profile, add as an ignore if it is a parent profile
            var matchingActiveConfig = resource.ConfigSets.FirstOrDefault(x => x.Id == item.Id);
            if (matchingActiveConfig is null) continue;
            
            resource.ConfigSets = resource.ConfigSets.Where(x => x.Id != item.Id).ToList();
        }
        
        // Remove this config item from updated if it was updated and is now being deleted
        var matchingUpdateConfig = _updatedConfigItems.FirstOrDefault(x => x.Id == item.Id);
        if (matchingUpdateConfig is not null)
        {
            _updatedConfigItems.Remove(item);
        }
        
        // Add the config item to the update list to delete when saved
        var matchingDeleteConfig = _deletedConfigItems.FirstOrDefault(x => x.Id == item.Id);
        if (matchingDeleteConfig is null)
        {
            _deletedConfigItems.Add(item);
        }
    }
}   