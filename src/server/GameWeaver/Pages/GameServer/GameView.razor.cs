using System.Xml.Linq;
using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Helpers.GameServer;
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
using Domain.Enums.Integrations;
using GameWeaver.Components.GameServer;
using GameWeaver.Helpers;
using GameWeaverShared.Parsers;
using Microsoft.AspNetCore.Components.Forms;

namespace GameWeaver.Pages.GameServer;

public partial class GameView : ComponentBase
{
    [Parameter] public Guid GameId { get; set; } = Guid.Empty;

    [Inject] public IGameService GameService { get; init; } = null!;
    [Inject] public IGameServerService GameServerService { get; init; } = null!;
    [Inject] public IFileStorageRecordService FileStorageService { get; init; } = null!;
    [Inject] public IWebClientService WebClientService { get; init; } = null!;
    [Inject] public ISerializerService SerializerService { get; init; } = null!;

    private readonly List<ConfigurationItemSlim> _createdConfigItems = [];
    private readonly List<LocalResourceSlim> _createdLocalResources = [];
    private readonly List<ConfigurationItemSlim> _deletedConfigItems = [];
    private readonly List<LocalResourceSlim> _deletedLocalResources = [];
    private readonly List<ConfigurationItemSlim> _updatedConfigItems = [];
    private readonly List<LocalResourceSlim> _updatedLocalResources = [];
    private readonly List<Guid> _viewableGameServers = [];
    private string _configSearchText = string.Empty;
    private string _editButtonText = "Enable Edit Mode";
    private bool _editMode;
    private GameSlim _game = new() {Id = Guid.Empty};
    private List<LocalResourceSlim> _localResources = [];
    private Guid _loggedInUserId = Guid.Empty;
    private List<FileStorageRecordSlim> _manualVersionFiles = [];
    private List<GameServerSlim> _runningGameservers = [];
    private bool _validIdProvided = true;

    private bool _canConfigureGame;
    private bool _canEditGame;
    private bool _canViewGameFiles;
    private bool _canViewGameServers;
    private bool _canExportGame;


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            // TODO: Handle Version URL & Manual file uploads
            if (firstRender)
            {
                await GetPermissions();
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
        _canViewGameServers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.SeeUi);
        _canViewGameFiles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.GameVersions.Get);
        _canExportGame = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Game.Export);
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
        if (!_canConfigureGame && !_canEditGame)
        {
            return;
        }

        var response = await GameService.UpdateAsync(_game.ToUpdate(), _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var resource in _deletedLocalResources)
        {
            var deleteResourceResponse = await GameServerService.DeleteLocalResourceAsync(resource.Id, _loggedInUserId);
            if (deleteResourceResponse.Succeeded) continue;

            deleteResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _deletedLocalResources.Clear();

        foreach (var resource in _createdLocalResources)
        {
            resource.PathWindows = FileHelpers.SanitizeSecureFilename(resource.PathWindows);
            resource.PathLinux = FileHelpers.SanitizeSecureFilename(resource.PathLinux);
            resource.PathMac = FileHelpers.SanitizeSecureFilename(resource.PathMac);
            var createResourceRequest = resource.ToCreate();
            createResourceRequest.Id = resource.Id;
            var createResourceResponse = await GameServerService.CreateLocalResourceAsync(createResourceRequest, _loggedInUserId);
            if (createResourceResponse.Succeeded) continue;

            createResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _createdLocalResources.Clear();

        if (_createdConfigItems.Count != 0 || _updatedConfigItems.Count != 0 || _deletedConfigItems.Count != 0)
        {
            if (await SaveNewConfigItems())
            {
                return;
            }

            _createdConfigItems.Clear();
            if (await SaveUpdatedConfigItems())
            {
                return;
            }

            _updatedConfigItems.Clear();
            if (await SaveDeletedConfigItems())
            {
                return;
            }

            _deletedConfigItems.Clear();
        }

        foreach (var resource in _updatedLocalResources)
        {
            resource.PathWindows = FileHelpers.SanitizeSecureFilename(resource.PathWindows);
            resource.PathLinux = FileHelpers.SanitizeSecureFilename(resource.PathLinux);
            resource.PathMac = FileHelpers.SanitizeSecureFilename(resource.PathMac);
            var updateResourceResponse = await GameServerService.UpdateLocalResourceAsync(resource.ToUpdate(), _loggedInUserId);
            if (updateResourceResponse.Succeeded) continue;

            updateResourceResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _updatedLocalResources.Clear();

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

    private async Task ConfigAdd(LocalResourceSlim localResource)
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters {{"ReferenceResource", localResource}};
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

        // If config was just created, we'll just remove it and move on
        var matchingCreatedConfig = _createdConfigItems.FirstOrDefault(x => x.Id == item.Id);
        if (matchingCreatedConfig is not null)
        {
            _createdConfigItems.Remove(item);
            return;
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

    private async Task LocalResourceAdd(ResourceType resourceType)
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Medium, CloseOnEscapeKey = true, FullWidth = true};
        var dialogParameters = new DialogParameters {{"GameProfileId", _game.DefaultGameProfileId}, {"ResourceType", resourceType}};
        var dialog = await DialogService.ShowAsync<LocalResourceAddDialog>("New Local Resource", dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var newResource = (LocalResourceSlim) dialogResult.Data;
        var startupResourceCount = _localResources.Count(x => x.Type is ResourceType.Executable or ResourceType.ScriptFile);
        newResource.StartupPriority = startupResourceCount; // We start from 0 for priority so the count is correct
        newResource.Startup = true; // We'll assume since we just created it, we would want this resource enabled

        _localResources.Add(newResource);
        _createdLocalResources.Add(newResource);
    }

    private void LocalResourceUpdate(LocalResourceSlim localResource)
    {
        var matchingNewResource = _createdLocalResources.FirstOrDefault(x => x.Id == localResource.Id);
        if (matchingNewResource is not null)
        {
            matchingNewResource.Name = localResource.Name;
            matchingNewResource.PathWindows = localResource.PathWindows;
            matchingNewResource.PathLinux = localResource.PathLinux;
            matchingNewResource.PathMac = localResource.PathMac;
            matchingNewResource.Args = localResource.Args;
            return;
        }

        var matchingUpdatedResource = _updatedLocalResources.FirstOrDefault(x => x.Id == localResource.Id);
        if (matchingUpdatedResource is null)
        {
            _updatedLocalResources.Add(localResource);
            return;
        }

        matchingUpdatedResource.Name = localResource.Name;
        matchingUpdatedResource.PathWindows = localResource.PathWindows;
        matchingUpdatedResource.PathLinux = localResource.PathLinux;
        matchingUpdatedResource.PathMac = localResource.PathMac;
        matchingUpdatedResource.Args = localResource.Args;
    }

    private async Task LocalResourceDelete(LocalResourceSlim localResource)
    {
        var dialogOptions = new DialogOptions {CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true};
        var dialogParameters = new DialogParameters
        {
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

        // Config items have a foreign key cascade delete, so we will just remove any changes for this resource
        foreach (var configItem in localResource.ConfigSets)
        {
            var matchingNewConfig = _createdConfigItems.FirstOrDefault(x => x.Id == configItem.Id);
            if (matchingNewConfig is not null)
            {
                _createdConfigItems.Remove(matchingNewConfig);
            }

            var matchingUpdatedConfig = _createdConfigItems.FirstOrDefault(x => x.Id == configItem.Id);
            if (matchingUpdatedConfig is not null)
            {
                _updatedConfigItems.Remove(matchingUpdatedConfig);
            }

            var matchingDeletedConfig = _createdConfigItems.FirstOrDefault(x => x.Id == configItem.Id);
            if (matchingDeletedConfig is not null)
            {
                _deletedConfigItems.Remove(matchingDeletedConfig);
            }
        }

        var matchingNewResource = _createdLocalResources.FirstOrDefault(x => x.Id == localResource.Id);
        if (matchingNewResource is not null)
        {
            _createdLocalResources.Remove(matchingNewResource);
            _localResources.Remove(localResource);
            return;
        }

        var matchingUpdatedResource = _updatedLocalResources.FirstOrDefault(x => x.Id == localResource.Id);
        if (matchingUpdatedResource is not null)
        {
            _updatedLocalResources.Remove(matchingUpdatedResource);
        }

        _localResources.Remove(localResource);
        _deletedLocalResources.Add(localResource);
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
                resourceCreateRequest.PathWindows = FileHelpers.SanitizeSecureFilename(resourceCreateRequest.PathWindows);
                resourceCreateRequest.PathLinux = FileHelpers.SanitizeSecureFilename(resourceCreateRequest.PathLinux);
                resourceCreateRequest.PathMac = FileHelpers.SanitizeSecureFilename(resourceCreateRequest.PathMac);
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

    private async Task OpenScriptInEditor(LocalResourceSlim resource)
    {
        if (resource.Type != ResourceType.ScriptFile)
        {
            return;
        }

        var usablePath = resource switch
        {
            _ when !string.IsNullOrWhiteSpace(resource.PathWindows) => resource.PathWindows,
            _ when !string.IsNullOrWhiteSpace(resource.PathLinux) => resource.PathLinux,
            _ when !string.IsNullOrWhiteSpace(resource.PathMac) => resource.PathMac,
            _ => string.Empty
        };

        var scriptContent = string.Join(Environment.NewLine, resource.ConfigSets.ToRaw());
        var fileLanguage = FileHelpers.GetLanguageFromName(usablePath);
        var dialogResult = await DialogService.FileEditorDialog(usablePath, scriptContent, fileLanguage, _editMode, true);
        if (dialogResult.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var updatedFileContent = (string) dialogResult.Data;
        var fileContentLines = updatedFileContent.Split(Environment.NewLine);
        var editorConfigItems = resource.ContentType switch
        {
            ContentType.Raw => fileContentLines.ToConfigItems(resource.Id),
            ContentType.Ini => new IniData(fileContentLines).ToConfigItems(resource.Id),
            ContentType.Json => SerializerService.DeserializeJson<Dictionary<string, string>>(updatedFileContent).ToConfigItems(resource.Id),
            ContentType.Xml => XDocument.Parse(updatedFileContent).ToConfigItems(resource.Id),
            _ => fileContentLines.ToConfigItems(resource.Id)
        };

        // Updated raw file has desired state, we'll use existing ID's for existing items then add/delete based on provided state
        editorConfigItems.UpdateEditorConfigFromExisting(resource.ConfigSets);
        var newConfigItems = editorConfigItems.Where(x => resource.ConfigSets.FirstOrDefault(c => c.Id == x.Id) is null).ToList();
        var updateConfigItems = editorConfigItems.Where(x => resource.ConfigSets.FirstOrDefault(c => c.Id == x.Id) is not null).ToList();
        var deleteConfigItems = resource.ConfigSets.Where(x => editorConfigItems.FirstOrDefault(c => c.Id == x.Id) is null).ToList();
        resource.ConfigSets = editorConfigItems;

        AddOrUpdateConfiguration(_createdConfigItems, newConfigItems);
        AddOrUpdateConfiguration(_updatedConfigItems, updateConfigItems);
        AddOrUpdateConfiguration(_deletedConfigItems, deleteConfigItems);
        Snackbar.Add("Script updated, changes won't be made until you save", Severity.Warning);
        StateHasChanged();
    }

    private async Task OpenConfigInEditor(LocalResourceSlim resource)
    {
        if (resource.Type != ResourceType.ConfigFile)
        {
            return;
        }

        var usablePath = resource switch
        {
            _ when !string.IsNullOrWhiteSpace(resource.PathWindows) => resource.PathWindows,
            _ when !string.IsNullOrWhiteSpace(resource.PathLinux) => resource.PathLinux,
            _ when !string.IsNullOrWhiteSpace(resource.PathMac) => resource.PathMac,
            _ => string.Empty
        };

        var fileLanguage = resource.ContentType switch
        {
            ContentType.Ini => FileEditorLanguage.Ini,
            ContentType.Json => FileEditorLanguage.Json,
            ContentType.Xml => FileEditorLanguage.Xml,
            _ => FileHelpers.GetLanguageFromName(usablePath)
        };

        var configContent = resource.ContentType switch
        {
            ContentType.Raw => string.Join(Environment.NewLine, resource.ConfigSets.ToRaw()),
            ContentType.Ini => resource.ConfigSets.ToIni().ToString(),
            ContentType.Xml => resource.ConfigSets.ToXml()?.ToString() ?? string.Empty,
            _ => string.Join(Environment.NewLine, resource.ConfigSets.ToRaw())
        };
        var dialogResult = await DialogService.FileEditorDialog(usablePath, configContent, fileLanguage, _editMode, true);
        if (dialogResult.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var updatedFileContent = (string) dialogResult.Data;
        var fileContentLines = updatedFileContent.Split(Environment.NewLine);
        var configItems = resource.ContentType switch
        {
            ContentType.Raw => fileContentLines.ToConfigItems(resource.Id),
            ContentType.Ini => new IniData(fileContentLines).ToConfigItems(resource.Id),
            ContentType.Json => SerializerService.DeserializeJson<Dictionary<string, string>>(updatedFileContent).ToConfigItems(resource.Id),
            ContentType.Xml => XDocument.Parse(updatedFileContent).ToConfigItems(resource.Id),
            _ => fileContentLines.ToConfigItems(resource.Id)
        };

        // Updated raw file has desired state, we'll use existing ID's for existing items then add/delete based on provided state
        configItems.UpdateEditorConfigFromExisting(resource.ConfigSets);
        var newConfigItems = configItems.Where(x => resource.ConfigSets.FirstOrDefault(c => c.Id == x.Id) is null).ToList();
        var updateConfigItems = configItems.Where(x => resource.ConfigSets.FirstOrDefault(c => c.Id == x.Id) is not null).ToList();
        var deleteConfigItems = resource.ConfigSets.Where(x => configItems.FirstOrDefault(c => c.Id == x.Id) is null).ToList();
        resource.ConfigSets = configItems;

        AddOrUpdateConfiguration(_createdConfigItems, newConfigItems);
        AddOrUpdateConfiguration(_updatedConfigItems, updateConfigItems);
        AddOrUpdateConfiguration(_deletedConfigItems, deleteConfigItems);
        Snackbar.Add("Configuration file updated, changes won't be made until you save", Severity.Warning);
        StateHasChanged();
    }

    private static void AddOrUpdateConfiguration(List<ConfigurationItemSlim> existingItems, List<ConfigurationItemSlim> updatedItems)
    {
        foreach (var updatedItem in updatedItems)
        {
            var matchingItem = existingItems.FirstOrDefault(x => x.Id == updatedItem.Id);
            if (matchingItem is null)
            {
                existingItems.Add(updatedItem);
                continue;
            }

            matchingItem.FriendlyName = updatedItem.FriendlyName;
            matchingItem.Key = updatedItem.Key;
            matchingItem.Value = updatedItem.Value;
            matchingItem.Category = updatedItem.Category;
            matchingItem.Path = updatedItem.Path;
            matchingItem.DuplicateKey = updatedItem.DuplicateKey;
        }
    }

    private async Task ConfigSelectedForImport(IReadOnlyList<IBrowserFile?>? importFiles)
    {
        if (importFiles is null || !importFiles.Any())
        {
            return;
        }

        List<IBrowserFile> configToImport = [];
        foreach (var file in importFiles)
        {
            if (file is null)
            {
                continue;
            }

            if (file.Size > 10_000_000)
            {
                Snackbar.Add($"File is over the max size of 10MB: {file.Name}");
                continue;
            }

            configToImport.Add(file);
        }

        var confirmText = $"Are you sure you want to import these {configToImport.Count} files?{Environment.NewLine}" +
                          $"File paths can't be assumed so each will need to be updated manually before you save";
        var importConfirmation = await DialogService.ConfirmDialog($"Import {configToImport.Count} config files", confirmText);
        if (importConfirmation.Canceled)
        {
            return;
        }

        Snackbar.Add("Importing files, this may take a moment or two depending on file count and size...", Severity.Info);
        await ImportConfigFiles(configToImport);
    }

    private async Task ImportConfigFiles(IEnumerable<IBrowserFile> importFiles)
    {
        var importCount = 0;

        foreach (var file in importFiles)
        {
            var isNewConfigFile = true;
            var matchingResource = _localResources.FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x.PathWindows) && x.PathWindows.ToLower().EndsWith(file.Name.ToLower()) ||
                !string.IsNullOrWhiteSpace(x.PathLinux) && x.PathLinux.ToLower().EndsWith(file.Name.ToLower()) ||
                !string.IsNullOrWhiteSpace(x.PathMac) && x.PathMac.ToLower().EndsWith(file.Name.ToLower()));
            if (matchingResource is not null)
            {
                var confirmText = $"The existing resource {matchingResource.Name} will be replaced if you import this file: {file.Name}, are you sure you want to continue?";
                var replaceConfirmation = await DialogService.ConfirmDialog("Replace existing config file?", confirmText);
                if (replaceConfirmation.Canceled)
                {
                    Snackbar.Add($"File {file.Name} import was cancelled", Severity.Warning);
                    continue;
                }

                isNewConfigFile = false;
            }

            if (isNewConfigFile)
                matchingResource = new LocalResourceSlim
                {
                    Id = Guid.CreateVersion7(),
                    GameProfileId = _game.DefaultGameProfileId,
                    Name = file.Name.Split('.').First(),
                    PathWindows = _game.SupportsWindows ? file.Name : string.Empty,
                    PathLinux = _game.SupportsLinux ? file.Name : string.Empty,
                    PathMac = _game.SupportsMac ? file.Name : string.Empty,
                    Startup = false,
                    StartupPriority = 0,
                    Type = ResourceType.ConfigFile,
                    ContentType = FileHelpers.GetContentTypeFromName(file.Name),
                    Args = string.Empty,
                    LoadExisting = false
                };

            var fileContent = await file.GetContent(); // The max import size per file is 10MB by default
            if (!fileContent.Succeeded || fileContent.Data is null)
            {
                fileContent.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                continue;
            }

            var fileContentLines = fileContent.Data.Split(Environment.NewLine);
            List<ConfigurationItemSlim> configItems;
            try
            {
                configItems = matchingResource!.ContentType switch
                {
                    ContentType.Raw => fileContentLines.ToConfigItems(matchingResource.Id),
                    ContentType.Ini => new IniData(fileContentLines).ToConfigItems(matchingResource.Id),
                    ContentType.Json => SerializerService.DeserializeJson<List<ConfigurationItemExport>>(fileContent.Data).ToConfigItems(matchingResource.Id),
                    ContentType.Xml => XDocument.Parse(fileContent.Data).ToConfigItems(matchingResource.Id),
                    _ => fileContentLines.ToConfigItems(matchingResource.Id)
                };
            }
            catch (Exception)
            {
                Snackbar.Add($"File {file.Name} import failed due to being in an unsupported or corrupt format", Severity.Error);
                continue;
            }

            matchingResource.ConfigSets = matchingResource.ConfigSets.MergeConfiguration(configItems, true);
            foreach (var configItem in configItems)
            {
                // Set any config items not merged into existing items to be created
                if (matchingResource.ConfigSets.FirstOrDefault(x => x.Id == configItem.Id) is not null)
                {
                    _createdConfigItems.Add(configItem);
                    continue;
                }

                // Any config items where the id doesn't exist are updated
                var updatedConfigItem = matchingResource.ConfigSets.FirstOrDefault(x =>
                    (x.DuplicateKey && x.Category == configItem.Category && x.Key == configItem.Key && x.Path == configItem.Path && x.Value == configItem.Value) ||
                    (x.Category == configItem.Category && x.Key == configItem.Key && x.Path == configItem.Path));
                if (updatedConfigItem is null) continue;

                _updatedConfigItems.Add(updatedConfigItem);
            }

            importCount++;

            if (isNewConfigFile)
            {
                _createdLocalResources.Add(matchingResource);
                _localResources.Add(matchingResource);
                continue;
            }

            _updatedLocalResources.Add(matchingResource);
        }

        if (importCount <= 0)
        {
            Snackbar.Add("No files were imported", Severity.Info);
            return;
        }

        Snackbar.Add($"Successfully imported {importCount} configuration file(s), changes won't be made until you save", Severity.Success);
    }

    private async Task ExportProfile()
    {
        var gameProfileResponse = await GameServerService.GetGameProfileByIdAsync(_game.DefaultGameProfileId);
        if (!gameProfileResponse.Succeeded || gameProfileResponse.Data is null)
        {
            gameProfileResponse.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        var profileExport = gameProfileResponse.Data.ToExport(_game.SourceType is GameSource.Steam ? _game.SteamToolId.ToString() : _game.FriendlyName);
        foreach (var resource in _localResources)
        {
            var resourceExport = resource.ToExport();
            resourceExport.Configuration = resource.ConfigSets.Select(x => x.ToExport()).ToList();
            profileExport.Resources.Add(resourceExport);
        }

        var serializedProfile = SerializerService.SerializeJson(profileExport);
        var profileExportName = $"{FileHelpers.SanitizeSecureFilename(profileExport.Name)}.json";
        var downloadResult = await WebClientService.InvokeFileDownload(serializedProfile, profileExportName, DataConstants.MimeTypes.Json);
        if (!downloadResult.Succeeded)
        {
            downloadResult.Messages.ForEach(x => Snackbar.Add(x));
            return;
        }

        Snackbar.Add($"Successfully Exported Game Profile: {profileExportName}");
    }

    private async Task ExportGame()
    {
        if (!_canExportGame)
        {
            return;
        }

        var gameExport = _game.ToExport();
        var gameDefaultProfileRequest = await GameServerService.GetGameProfileByIdAsync(_game.DefaultGameProfileId);
        if (!gameDefaultProfileRequest.Succeeded || gameDefaultProfileRequest.Data is null)
        {
            gameDefaultProfileRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        var profileExport = gameDefaultProfileRequest.Data.ToExport(_game);
        var profileResourcesRequest = await GameServerService.GetLocalResourcesByGameProfileIdAsync(gameDefaultProfileRequest.Data.Id);
        if (!profileResourcesRequest.Succeeded)
        {
            profileResourcesRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var resource in profileResourcesRequest.Data)
        {
            var configItemsRequest = await GameServerService.GetConfigurationItemsByLocalResourceIdAsync(resource.Id);
            if (!configItemsRequest.Succeeded)
            {
                configItemsRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            var resourceExport = resource.ToExport();
            resourceExport.Configuration = configItemsRequest.Data.Select(x => x.ToExport()).ToList();
            profileExport.Resources.Add(resourceExport);
        }

        gameExport.DefaultGameProfile = profileExport;
        var serializedExport = SerializerService.SerializeJson(gameExport);
        var exportName = $"Game_{FileHelpers.SanitizeSecureFilename(_game.FriendlyName)}.json";
        var downloadResult = await WebClientService.InvokeFileDownload(serializedExport, exportName, DataConstants.MimeTypes.Json);
        if (!downloadResult.Succeeded)
        {
            downloadResult.Messages.ForEach(x => Snackbar.Add(x));
            return;
        }

        Snackbar.Add($"Successfully Exported Game: {exportName}", Severity.Success);
    }
}