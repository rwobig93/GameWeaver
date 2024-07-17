using Application.Models.GameServer.Game;
using Application.Services.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class Games : ComponentBase
{
    [Inject] private IGameService GameService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; set; } = null!;
    
    private MudTable<GameSlim> _table = new();
    private IEnumerable<GameSlim> _pagedData = [];
    
    private int _totalItems = 10;
    private int _totalPages = 1;
    private int _pageSize = 25;
    private int _pageNumber;
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
        // TODO: Add paginated detail to paginated method return
        var games = await GameService.GetAllPaginatedAsync(_pageNumber, _pageSize);
        if (!games.Succeeded)
        {
            games.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _pagedData = games.Data;
        _totalItems = (await GameService.GetCountAsync()).Data;
        _totalPages = _totalItems / _pageSize;
        GetCurrentPageViewData();
        StateHasChanged();
    }
    
    private string GetCurrentPageViewData()
    {
        var currentStart = 1;
        var currentEnd = _pageSize * _pageNumber;

        if (_pageNumber > 1)
        {
            currentStart += (_pageNumber - 1) * _pageSize;
        }

        if (_pageSize * _pageNumber > _totalItems)
            currentEnd = _totalItems;

        return $"{currentStart}-{currentEnd} of {_totalItems}";
    }
    
    private async Task PageChanged(int pageNumber)
    {
        _pageNumber = pageNumber;
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
    }
}