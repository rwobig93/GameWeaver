using Application.Models.GameServer.Host;
using Application.Services.GameServer;
using Microsoft.AspNetCore.Components;

namespace GameWeaver.Pages.GameServer;

public partial class HostsDashboard : ComponentBase
{
    [Inject] private IHostService HostService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; set; } = null!;
    
    private MudTable<HostSlim> _table = new();
    private IEnumerable<HostSlim> _pagedData = new List<HostSlim>();
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    
    private int _totalItems = 10;
    private int _totalPages = 1;
    private int _pageSize = 25;
    private int _pageNumber;
    // private readonly string[] _orderings = null;
    // private string _searchString = "";
    // private List<string> _autocompleteList;
    
    private async Task RefreshData()
    {
        var hosts = await HostService.GetAllPaginatedAsync(_pageNumber, _pageSize);
        if (!hosts.Succeeded)
        {
            hosts.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _pagedData = hosts.Data;
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
        await WebClientService.InvokeScrollToTop();
    }
}