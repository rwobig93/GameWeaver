using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.GameServer.GameServer;
using Application.Services.GameServer;
using GameWeaver.Components.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class GameServers : ComponentBase
{
    [Inject] private IGameServerService GameServerService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; set; } = null!;
    
    private IEnumerable<GameServerSlim> _pagedData = [];
    
    private string _searchText = "";
    private int _totalItems = 10;
    private int _totalPages = 1;
    private int _pageSize = PaginationHelpers.GetPageSizes(true).First();
    private int _currentPage = 1;
    // private readonly string[] _orderings = null;
    // private string _searchString = "";
    // private List<string> _autocompleteList;
    
    private bool _canCreateGameServers = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            await RefreshData();
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canCreateGameServers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Create);
    }
    
    private async Task RefreshData()
    {
        var response = await GameServerService.SearchPaginatedAsync(_searchText, _currentPage, _pageSize);
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

    private async Task CreateServer()
    {
        if (!_canCreateGameServers)
        {
            return;
        }
        
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var dialog = await DialogService.Show<GameServerCreateDialog>("Create Gameserver", new DialogParameters(), dialogOptions).Result;
        if (dialog?.Data is null || dialog.Canceled)
        {
            return;
        }

        var createdGameServerId = (Guid) dialog.Data;
        Snackbar.Add("Successfully created new gameserver!", Severity.Success);
        NavManager.NavigateTo(AppRouteConstants.GameServer.GameServers.ViewId(createdGameServerId));
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