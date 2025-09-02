using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Helpers.GameServer;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Models.GameServer.Game;
using Application.Models.GameServer.GameProfile;
using Application.Models.GameServer.LocalResource;
using Application.Services.GameServer;
using Domain.Models.Identity;
using GameWeaver.Components.GameServer;
using GameWeaver.Helpers;
using Microsoft.AspNetCore.Components.Forms;

namespace GameWeaver.Pages.GameServer;

public partial class Games : ComponentBase
{
    [Inject] private IGameService GameService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    [Inject] private IAppAccountService AccountService { get; init; } = null!;
    [Inject] private ISerializerService SerializerService { get; init; } = null!;
    [Inject] private IGameServerService GameServerService { get; init; } = null!;

    private IEnumerable<GameSlim> _pagedData = [];
    private List<GameWidget> _gameWidgets = [];

    public GameWidget WidgetReference
    {
        set => _gameWidgets.Add(value);
    }

    private Guid _loggedInUserId = Guid.Empty;
    private AppUserPreferenceFull _userPreferences = new();
    private string _searchText = "";
    private int _totalItems = 10;
    private int _totalPages = 1;
    private int _pageSize = PaginationHelpers.GetPageSizes(true).First();
    private int _currentPage = 1;
    private bool _showNames;
    public readonly string _cssDisplay = "game-card-lift";
    private string _importCurrentContext = string.Empty;
    private int _importProgressCurrent;
    private int _importProgressTotal;

    private bool _canCreateGames;
    private bool _canImportGames;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            await GetUserPreferences();
            await RefreshData();
        }
    }

    private async Task RefreshData()
    {
        _pagedData = [];
        _gameWidgets = [];

        // Searching by clicking the button works as expected, pressing "enter" in the text field results in the previous results, enter has to be hit twice
        // Assuming some form of caching I haven't been able to find yet is the cause, adding another small get call works for now though
        await GameService.GetAllPaginatedAsync(1, 1);
        var response = await GameService.SearchPaginatedAsync(_searchText, _currentPage, _pageSize);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _pagedData = response.Data;
        _totalItems = response.TotalCount;
        _totalPages = response.EndPage;
        GetCurrentPageViewData();
        StateHasChanged();
    }

    private async Task GetUserPreferences()
    {
        if (_loggedInUserId == Guid.Empty)
        {
            return;
        }

        _userPreferences = (await AccountService.GetPreferences(_loggedInUserId)).Data;
        StateHasChanged();
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _loggedInUserId = CurrentUserService.GetIdFromPrincipal(currentUser);

        _canCreateGames = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Game.Create);
        _canImportGames = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Game.Import);
        StateHasChanged();
    }

    private async Task UpdateGameWidgets()
    {
        foreach (var gameWidget in _gameWidgets)
        {
            await gameWidget.UpdateImage();
        }
    }

    private string GetCurrentPageViewData()
    {
        var currentStart = 1;
        var currentEnd = _pageSize * _currentPage;

        if (_totalItems == 0)
        {
            currentStart = 0;
        }

        if (_currentPage > 1)
        {
            currentStart += (_currentPage - 1) * _pageSize;
        }

        if (_pageSize * _currentPage > _totalItems)
            currentEnd = _totalItems;

        return $"{currentStart}-{currentEnd} of {_totalItems}";
    }

    private async Task PageChanged(int pageNumber)
    {
        _currentPage = pageNumber;
        await RefreshData();
    }

    private async Task ToggleNames()
    {
        _showNames = !_showNames;
        StateHasChanged();
        await UpdateGameWidgets();
    }

    private async Task CreateGame()
    {
        await Task.CompletedTask;
        Snackbar.Add("Not currently implemented", Severity.Warning);
    }

    private async Task SearchKeyDown(KeyboardEventArgs keyArgs)
    {
        if (keyArgs.Code is "Enter" or "NumpadEnter")
        {
            await RefreshData();
        }
    }

    private async Task PageSizeChanged()
    {
        await RefreshData();
        await WebClientService.InvokeScrollToTop();
    }

    private async Task GamesSelectedForImport(IReadOnlyList<IBrowserFile>? importFiles)
    {
        if (!_canImportGames)
        {
            return;
        }

        if (importFiles is null || !importFiles.Any())
        {
            return;
        }

        List<IBrowserFile> filesToImport = [];
        foreach (var file in importFiles)
        {
            if (file.Size > 10_000_000)
            {
                Snackbar.Add($"File is over the max size of 10MB: {file.Name}", Severity.Error);
                continue;
            }

            filesToImport.Add(file);
        }

        var confirmText = $"Are you sure you want to import these {filesToImport.Count} games?{Environment.NewLine}" +
                          $"Any existing games with the same name will be overwritten";
        var importConfirmation = await DialogService.ConfirmDialog($"Import {filesToImport.Count} games", confirmText);
        if (importConfirmation.Canceled)
        {
            return;
        }

        Snackbar.Add("Importing files, this may take a moment or two depending on file count and size...", Severity.Info);
        _importProgressCurrent = 0;
        _importProgressTotal = filesToImport.Count;
        StateHasChanged();

        foreach (var file in filesToImport)
        {
            _importCurrentContext = file.Name;
            _importProgressCurrent++;
            StateHasChanged();

            var fileContent = await file.GetContent(); // The max import size per file is 10MB by default
            if (!fileContent.Succeeded || fileContent.Data is null)
            {
                fileContent.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                return;
            }

            GameExport deserializedGame;
            try
            {
                deserializedGame = SerializerService.DeserializeJson<GameExport>(fileContent.Data);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Game import from file failed: [{FileName}]{Error}", file.Name, ex.Message);
                Snackbar.Add($"File {file.Name} import failed due to being in an unsupported or corrupt format", Severity.Error);
                return;
            }

            await ImportGame(deserializedGame);
        }

        Snackbar.Add($"Finished importing {filesToImport.Count} games", Severity.Success);
        _importCurrentContext = string.Empty;
        _importProgressCurrent = 0;
        _importProgressTotal = 0;
        StateHasChanged();
    }

    private async Task ImportGame(GameExport gameExport)
    {
        if (!_canImportGames)
        {
            return;
        }

        var matchingGameRequest = await GameService.GetByFriendlyNameAsync(gameExport.FriendlyName);
        if (matchingGameRequest.Data is null)
        {
            var createGameRequest = await GameService.CreateAsync(gameExport.ToCreate(), _loggedInUserId);
            if (createGameRequest.Succeeded) return;

            createGameRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        var updateGameRequest = await GameService.UpdateAsync(gameExport.ToUpdate(matchingGameRequest.Data.Id), _loggedInUserId);
        if (!updateGameRequest.Succeeded)
        {
            updateGameRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
        }

        await ImportProfile(matchingGameRequest.Data, gameExport.DefaultGameProfile);
    }

    private async Task ImportProfile(GameSlim game, GameProfileExport profileExport)
    {
        var matchingProfileRequest = await GameServerService.GetGameProfileByIdAsync(game.DefaultGameProfileId);
        if (matchingProfileRequest.Data is null)
        {
            Snackbar.Add("Failed to find the game profile for the newly created game, please reach out to an administrator", Severity.Error);
            return;
        }

        var updateProfileRequest = await GameServerService.UpdateGameProfileAsync(profileExport.ToUpdateRequest(matchingProfileRequest.Data.Id), _loggedInUserId);
        if (!updateProfileRequest.Succeeded)
        {
            updateProfileRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        await ImportConfigResources(matchingProfileRequest.Data!.Id, profileExport.Resources);
    }

    private async Task ImportConfigResources(Guid profileId, List<LocalResourceExport> importResources)
    {
        var existingResourcesRequest = await GameServerService.GetLocalResourcesByGameProfileIdAsync(profileId);
        if (!existingResourcesRequest.Succeeded)
        {
            existingResourcesRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        foreach (var resource in importResources)
        {
            var matchingResource = existingResourcesRequest.Data.FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x.PathWindows) && string.Equals(x.PathWindows, resource.PathWindows, StringComparison.CurrentCultureIgnoreCase) ||
                !string.IsNullOrWhiteSpace(x.PathLinux) && string.Equals(x.PathLinux, resource.PathLinux, StringComparison.CurrentCultureIgnoreCase) ||
                !string.IsNullOrWhiteSpace(x.PathMac) && string.Equals(x.PathMac, resource.PathMac, StringComparison.CurrentCultureIgnoreCase));

            // Create the resource if it doesn't exist
            if (matchingResource is null)
            {
                var createResourceRequest = await GameServerService.CreateLocalResourceAsync(resource.ToCreate(), _loggedInUserId);
                if (!createResourceRequest.Succeeded)
                {
                    createResourceRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return;
                }

                var createdResourceRequest = await GameServerService.GetLocalResourceByIdAsync(createResourceRequest.Data);
                if (!createdResourceRequest.Succeeded)
                {
                    createdResourceRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return;
                }

                matchingResource = createdResourceRequest.Data;
            }

            // Create or update all config items from the resource import
            var importConfigItems = resource.Configuration.Select(x => x.ToSlim()).ToList();
            matchingResource.ConfigSets = matchingResource.ConfigSets.MergeConfiguration(importConfigItems, true);
            foreach (var configItem in importConfigItems)
            {
                // Set any config items not merged into existing items to be created
                configItem.LocalResourceId = matchingResource.Id;
                if (matchingResource.ConfigSets.FirstOrDefault(x => x.Id == configItem.Id) is not null)
                {
                    var createConfigItemRequest = await GameServerService.CreateConfigurationItemAsync(configItem.ToCreate(), _loggedInUserId);
                    if (!createConfigItemRequest.Succeeded)
                    {
                        createConfigItemRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                        return;
                    }

                    continue;
                }

                // Any config items where the id doesn't exist are updated
                var updatedConfigItem = matchingResource.ConfigSets.FirstOrDefault(x =>
                    (x.DuplicateKey && x.Category == configItem.Category && x.Key == configItem.Key && x.Path == configItem.Path && x.Value == configItem.Value) ||
                    (x.Category == configItem.Category && x.Key == configItem.Key && x.Path == configItem.Path));
                if (updatedConfigItem is null)
                {
                    continue;
                }

                configItem.Id = updatedConfigItem.Id;
                var updateConfigItemRequest = await GameServerService.UpdateConfigurationItemAsync(configItem.ToUpdate(), _loggedInUserId);
                if (updateConfigItemRequest.Succeeded) continue;

                updateConfigItemRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            }
        }
    }
}