using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.ConfigurationItem;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.LocalResource;
using Application.Models.Integrations;
using Application.Services.GameServer;
using Application.Services.Integrations;
using Domain.Enums.GameServer;
using Domain.Enums.Identity;
using GameWeaver.Components.GameServer;

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
    private List<LocalResourceSlim> _localResources = [];
    private string _configSearchText = string.Empty;
    private readonly List<ConfigurationItemSlim> _createdConfigItems = [];
    private readonly List<ConfigurationItemSlim> _updatedConfigItems = [];
    private readonly List<ConfigurationItemSlim> _deletedConfigItems = [];
    private readonly List<LocalResourceSlim> _createdLocalResources = [];
    private readonly List<LocalResourceSlim> _updatedLocalResources = [];
    private readonly List<LocalResourceSlim> _deletedLocalResources = [];
    private readonly List<Guid> _viewableGameServers = [];

    private bool _canEditGame;
    private bool _canConfigureGame;
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
                await GetGameProfileResources();
                StateHasChanged();
            }
        }
        catch
        {
            StateHasChanged();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _loggedInUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        _canEditGame = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Game.Update);
        _canConfigureGame = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Game.Configure);
        _canViewGameServers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get);
        _canViewGameFiles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.GameVersions.Get);
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
        var response = await GameServerService.GetByGameIdAsync(_game.Id, _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _runningGameservers = response.Data.ToList();

        foreach (var server in _runningGameservers)
        {
            if (await CanViewGameServer(server.Id))
            {
                _viewableGameServers.Add(server.Id);
            }
        }
    }

    private async Task GetGameProfileResources()
    {
        if (!_validIdProvided)
        {
            return;
        }

        var response = await GameServerService.GetLocalResourcesByGameProfileIdAsync(_game.DefaultGameProfileId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _localResources = response.Data.ToList();
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

        foreach (var resource in _createdLocalResources)
        {
            var createResourceResponse = await GameServerService.CreateLocalResourceAsync(resource.ToCreate(), _loggedInUserId);
            if (createResourceResponse.Succeeded) continue;

            createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (_createdConfigItems.Count != 0 || _updatedConfigItems.Count != 0 || _deletedConfigItems.Count != 0)
        {
            if (await SaveNewConfigItems())
            {
                return;
            }
            if (await SaveUpdatedConfigItems())
            {
                return;
            }
            if (await SaveDeletedConfigItems())
            {
                return;
            }
        }

        foreach (var resource in _updatedLocalResources)
        {
            var updateResourceResponse = await GameServerService.UpdateLocalResourceAsync(resource.ToUpdate(), _loggedInUserId);
            if (!updateResourceResponse.Succeeded) continue;

            updateResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var resource in _deletedLocalResources)
        {
            var deleteResourceResponse = await GameServerService.DeleteLocalResourceAsync(resource.Id, _loggedInUserId);
            if (deleteResourceResponse.Succeeded) continue;

            deleteResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _createdConfigItems.Clear();
        _updatedConfigItems.Clear();
        _deletedConfigItems.Clear();
        _createdLocalResources.Clear();
        _updatedLocalResources.Clear();
        _deletedLocalResources.Clear();

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
        Snackbar.Add("This feature is currently not implemented", Severity.Warning);
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

    private bool ConfigShouldBeShown(ConfigurationItemSlim item)
    {
        var shouldBeShown = item.FriendlyName.Contains(_configSearchText, StringComparison.OrdinalIgnoreCase) ||
                            item.Key.Contains(_configSearchText, StringComparison.OrdinalIgnoreCase) ||
                            item.Value.Contains(_configSearchText, StringComparison.OrdinalIgnoreCase);

        return shouldBeShown;
    }

    private async Task ConfigAdd(LocalResourceSlim localResource)
    {
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var dialogParameters = new DialogParameters() {{"ReferenceResource", localResource}};
        var dialog = await DialogService.ShowAsync<ConfigAddDialog>("Add Config Item", dialogParameters, dialogOptions);
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
            matchingNewConfig.FriendlyName = item.FriendlyName;
            matchingNewConfig.Key = item.Key;
            matchingNewConfig.Value = item.Value;
            matchingNewConfig.Category = item.Category;
            matchingNewConfig.Path = item.Path;
            matchingNewConfig.DuplicateKey = item.DuplicateKey;
            return;
        }

        var matchingUpdateConfig = _updatedConfigItems.FirstOrDefault(x => x.Id == item.Id);
        if (matchingUpdateConfig is null)
        {
            _updatedConfigItems.Add(item);
            return;
        }

        matchingUpdateConfig.FriendlyName = item.FriendlyName;
        matchingUpdateConfig.Key = item.Key;
        matchingUpdateConfig.Value = item.Value;
        matchingUpdateConfig.Category = item.Category;
        matchingUpdateConfig.Path = item.Path;
        matchingUpdateConfig.DuplicateKey = item.DuplicateKey;
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
            // ReSharper disable once PossibleMultipleEnumeration
            var resourceConfigSets = resource.ConfigSets.ToList();
            var matchingActiveConfig = resourceConfigSets.FirstOrDefault(x => x.Id == item.Id);
            if (matchingActiveConfig is null) continue;

            // If the config item is from a parent profile, add as an ignore to the existing resource on the direct profile instead
            if (matchingActiveConfig.LocalResourceId == Guid.Empty)
            {
                matchingActiveConfig.Id = Guid.CreateVersion7();
                matchingActiveConfig.LocalResourceId = resource.Id;
                matchingActiveConfig.Ignore = true;
                _createdConfigItems.Add(matchingActiveConfig);
                resource.ConfigSets = resourceConfigSets.ToList().Prepend(matchingActiveConfig);
                continue;
            }

            resource.ConfigSets = resourceConfigSets.Where(x => x.Id != item.Id).ToList();
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

    private async Task LocalResourceAdd()
    {
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var dialogParameters = new DialogParameters() {{"GameProfileId", _game.DefaultGameProfileId}};
        var dialog = await DialogService.ShowAsync<LocalResourceAddDialog>("New Local Resource", dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var newResource = (LocalResourceSlim) dialogResult.Data;

        _localResources.Add(newResource);
        _createdLocalResources.Add(newResource);
    }

    private void LocalResourceUpdate(LocalResourceSlim localResource)
    {
        var matchingNewResource = _createdLocalResources.FirstOrDefault(x => x.Id == localResource.Id);
        if (matchingNewResource is not null)
        {
            matchingNewResource.PathWindows = localResource.PathWindows;
            matchingNewResource.PathLinux = localResource.PathLinux;
            matchingNewResource.PathMac = localResource.PathMac;
            return;
        }

        var matchingUpdatedResource = _updatedLocalResources.FirstOrDefault(x => x.Id == localResource.Id);
        if (matchingUpdatedResource is null)
        {
            _updatedLocalResources.Add(localResource);
            return;
        }

        matchingUpdatedResource.PathWindows = localResource.PathWindows;
        matchingUpdatedResource.PathLinux = localResource.PathLinux;
        matchingUpdatedResource.PathMac = localResource.PathMac;
    }

    private async Task LocalResourceDelete(LocalResourceSlim localResource)
    {
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var dialogParameters = new DialogParameters() {
            {"Title", "Are you sure you want to delete this local resource?"},
            {"Content", $"You want to delete the resource '{localResource.Name}'?"}
        };
        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Delete Local Resource", dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var deleteResource = (bool) dialogResult.Data;
        if (!deleteResource)
        {
            return;
        }

        var matchingNewResource = _createdLocalResources.FirstOrDefault(x => x.Id == localResource.Id);
        if (matchingNewResource is not null)
        {
            _createdLocalResources.Remove(matchingNewResource);
        }

        var matchingUpdatedResource = _updatedLocalResources.FirstOrDefault(x => x.Id == localResource.Id);
        if (matchingUpdatedResource is not null)
        {
            _updatedLocalResources.Remove(matchingUpdatedResource);
        }

        _localResources.Remove(localResource);
        _deletedLocalResources.Add(localResource);
        foreach (var configItem in localResource.ConfigSets)
        {
            _deletedConfigItems.Remove(configItem);
        }
    }

    private async Task<bool> SaveNewConfigItems()
    {
        foreach (var configItem in _createdConfigItems)
        {
            // Check if there is a matching-ignored item (meaning deleted and to ignore from inherited config), if there is update it instead of creating a new item
            var matchingIgnoreItem = _localResources
                .Where(x => x.Id == configItem.LocalResourceId)
                .Select(x => x.ConfigSets.FirstOrDefault(c => c.Ignore && c.Key == configItem.Key)).FirstOrDefault();
            if (matchingIgnoreItem is not null)
            {
                matchingIgnoreItem.Ignore = false;
                matchingIgnoreItem.FriendlyName = configItem.FriendlyName;
                matchingIgnoreItem.Value = configItem.Value;
                matchingIgnoreItem.Category = configItem.Category;
                matchingIgnoreItem.Path = configItem.Path;
                _updatedConfigItems.Add(matchingIgnoreItem);
                continue;
            }

            var createConfigResponse = await GameServerService.CreateConfigurationItemAsync(configItem.ToCreate(), _loggedInUserId);
            if (createConfigResponse.Succeeded) continue;

            createConfigResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return true;
        }

        _createdConfigItems.Clear();
        return false;
    }

    private async Task<bool> SaveUpdatedConfigItems()
    {
        foreach (var configItem in _updatedConfigItems)
        {
            var matchingLocalResource = _localResources.First(x => x.ConfigSets.Any(c => c.Id == configItem.Id));
            var existingLocalResource = _localResources.FirstOrDefault(x => x.GameProfileId == _game.DefaultGameProfileId &&
                                                                            (x.PathWindows.Length != 0 && x.PathWindows == matchingLocalResource.PathWindows) ||
                                                                            (x.PathLinux.Length != 0 && x.PathLinux == matchingLocalResource.PathLinux) ||
                                                                            (x.PathMac.Length != 0 && x.PathMac == matchingLocalResource.PathMac));

            if (existingLocalResource is null)
            {
                var resourceCreateRequest = matchingLocalResource.ToCreate();
                resourceCreateRequest.GameProfileId = _game.DefaultGameProfileId;
                var createResourceResponse = await GameServerService.CreateLocalResourceAsync(resourceCreateRequest, _loggedInUserId);
                if (!createResourceResponse.Succeeded)
                {
                    createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return true;
                }

                configItem.LocalResourceId = createResourceResponse.Data;
            }
            else
            {
                configItem.LocalResourceId = existingLocalResource.Id;
            }

            var updateConfigResponse = await GameServerService.UpdateConfigurationItemAsync(configItem.ToUpdate(), _loggedInUserId);
            if (updateConfigResponse.Succeeded) continue;

            updateConfigResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return true;
        }

        _updatedConfigItems.Clear();
        return false;
    }

    private async Task<bool> SaveDeletedConfigItems()
    {
        foreach (var configItem in _deletedConfigItems)
        {
            // Config item comes from inherited profile resource, create server profile resource for the config item to be ignored
            if (configItem.LocalResourceId == Guid.Empty)
            {
                var matchingLocalResource = _localResources.First(x => x.ConfigSets.Any(c => c.Id == configItem.Id));
                var createResourceResponse = await GameServerService.CreateLocalResourceAsync(matchingLocalResource.ToCreate(), _loggedInUserId);
                if (!createResourceResponse.Succeeded)
                {
                    createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return true;
                }

                configItem.LocalResourceId = createResourceResponse.Data;
                configItem.Ignore = true;

                var ignoreCreateResponse = await GameServerService.CreateConfigurationItemAsync(configItem.ToCreate(), _loggedInUserId);
                if (!ignoreCreateResponse.Succeeded)
                {
                    ignoreCreateResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return true;
                }
            }

            var deleteConfigResponse = await GameServerService.DeleteConfigurationItemAsync(configItem.Id, _loggedInUserId);
            if (deleteConfigResponse.Succeeded) continue;

            deleteConfigResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return true;
        }

        _deletedConfigItems.Clear();
        return false;
    }

    private void ViewGameServer(Guid id)
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.GameServers.ViewId(id));
    }

    private async Task<bool> CanViewGameServer(Guid id)
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        if (await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get))
        {
            return true;
        }

        if (await AuthorizationService.UserHasDynamicPermission(currentUser, DynamicPermissionGroup.GameServers, DynamicPermissionLevel.Admin, id))
        {
            return true;
        }

        if (await AuthorizationService.UserHasDynamicPermission(currentUser, DynamicPermissionGroup.GameServers, DynamicPermissionLevel.Moderator, id))
        {
            return true;
        }

        if (await AuthorizationService.UserHasDynamicPermission(currentUser, DynamicPermissionGroup.GameServers, DynamicPermissionLevel.View, id))
        {
            return true;
        }

        return false;
    }

    private void InjectDynamicValue(ConfigurationItemSlim item, string value)
    {
        item.Value = value;
    }
}