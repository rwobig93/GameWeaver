using Application.Helpers.Runtime;
using Application.Models.GameServer.Game;
using Application.Services.GameServer;
using Domain.Models.Identity;
using GameWeaver.Components.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class Games : ComponentBase
{
    [Inject] private IGameService GameService { get; init; } = null!;
    [Inject] private IWebClientService WebClientService { get; init; } = null!;
    [Inject] private IAppAccountService AccountService { get; init; } = null!;

    private IEnumerable<GameSlim> _pagedData = [];
    private List<GameWidget> _gameWidgets = [];
    public GameWidget WidgetReference
    {
        set => _gameWidgets.Add(value);
    }

    private AppUserPreferenceFull _userPreferences = new();
    private string _searchText = "";
    private int _totalItems = 10;
    private int _totalPages = 1;
    private int _pageSize = PaginationHelpers.GetPageSizes(true).First();
    private int _currentPage = 1;
    private bool _showNames;
    private string _cssDisplay = "game-card-lift";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
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
        var currentUserId = CurrentUserService.GetCurrentUserId();
        if (currentUserId.Result is null)
        {
            return;
        }

        _userPreferences = (await AccountService.GetPreferences(currentUserId.Result.GetFromNullable())).Data;
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
}