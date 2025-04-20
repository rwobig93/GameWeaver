using Application.Constants.Identity;
using Application.Helpers.Auth;
using Application.Helpers.Runtime;
using Application.Models.GameServer.GameProfile;
using Application.Services.GameServer;
using Domain.Models.Identity;
using GameWeaver.Components.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class GameProfiles : ComponentBase
{
    [Inject] private IGameServerService GameServerService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    [Inject] private IAppAccountService AccountService { get; init; } = null!;

    private IEnumerable<GameProfileSlim> _pagedData = [];
    private AppUserPreferenceFull _userPreferences = new();
    private Guid _loggedInUserId = Guid.Empty;

    private string _searchText = "";
    private int _totalItems = 10;
    private int _totalPages = 1;
    private int _pageSize = PaginationHelpers.GetPageSizes(true).First();
    private int _currentPage = 1;

    private bool _canCreateProfiles;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            await GetUserPreferences();
            await RefreshData();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canCreateProfiles = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.GameProfile.Create);
    }

    private async Task GetUserPreferences()
    {
        var currentUserId = await CurrentUserService.GetCurrentUserId();
        if (currentUserId is null)
        {
            return;
        }

        _userPreferences = (await AccountService.GetPreferences(currentUserId.GetFromNullable())).Data;
    }

    private async Task RefreshData()
    {
        _pagedData = [];
        StateHasChanged();

        var response = await GameServerService.SearchGameProfilesPaginatedAsync(_searchText, _currentPage, _pageSize);
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

    private async Task CreateProfile()
    {
        if (!_canCreateProfiles)
        {
            return;
        }

        var dialogOptions = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        // TODO: Create new profile dialog
        var dialog = await DialogService.ShowAsync<GameServerCreateDialog>("Create Configuration Profile", new DialogParameters(), dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var createdProfileId = (Guid) dialogResult.Data;
        Snackbar.Add("Successfully created new configuration profile!", Severity.Success);
        NavManager.NavigateTo(AppRouteConstants.GameServer.GameProfiles.ViewId(createdProfileId));
    }

    private async Task ImportProfiles()
    {
        if (!_canCreateProfiles)
        {
            return;
        }

        await Task.CompletedTask;

        // TODO: Add import functionality
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
}