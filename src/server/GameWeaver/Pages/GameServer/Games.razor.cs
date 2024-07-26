using Application.Models.GameServer.Game;
using Application.Services.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class Games : ComponentBase
{
    [Inject] private IGameService GameService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; set; } = null!;
    
    private IEnumerable<GameSlim> _pagedData = [];
    
    private string _searchText = "";
    private int _totalItems = 10;
    private int _totalPages = 1;
    private int _pageSize = 25;
    private int _currentPage;
    // private readonly string[] _orderings = null;
    // private string _searchString = "";
    // private List<string> _autocompleteList;
    private bool _displayVertical;
    private bool _showNames;
    private string _cssDisplay = "game-card-lift";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await RefreshData();
        }
    }
    
    private async Task RefreshData()
    {
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
    
    private string GetCurrentPageViewData()
    {
        var currentStart = 1;
        var currentEnd = _pageSize * _currentPage;

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

    private void ChangeOrientation()
    {
        _displayVertical = !_displayVertical;
        StateHasChanged();
    }

    private void ChangeStyle()
    {
        _cssDisplay = _cssDisplay == "game-card-lift" ? "game-card-slide" : "game-card-lift";
        StateHasChanged();
    }

    private void ToggleNames()
    {
        _showNames = !_showNames;
        StateHasChanged();
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
}