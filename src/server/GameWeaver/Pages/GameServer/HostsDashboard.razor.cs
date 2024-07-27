using Application.Models.GameServer.Host;
using Application.Services.GameServer;
using GameWeaver.Components.GameServer;

namespace GameWeaver.Pages.GameServer;

public partial class HostsDashboard : ComponentBase, IAsyncDisposable
{
    [Inject] private IHostService HostService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; set; } = null!;
    
    private IEnumerable<HostSlim> _pagedData = new List<HostSlim>();
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private Timer? _timer;
    private List<HostWidget> _hostWidgets = [];
    public HostWidget WidgetReference
    {
        set => _hostWidgets.Add(value);
    }

    private string _searchText = "";
    private int _totalItems = 10;
    private int _totalPages = 1;
    private int _pageSize = 25;
    private int _pageNumber;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetClientTimezone();
            await RefreshData();
            
            _timer = new Timer(async _ => { await TimerDataUpdate(); }, null, 0, 1000);
            
            await Task.CompletedTask;
        }
    }

    private async Task TimerDataUpdate()
    {
        if (_hostWidgets.Count == 0)
        {
            return;
        }
        
        foreach (var widget in _hostWidgets)
        {
            try
            {
                if (widget.GetCheckinCount() == 0)
                {
                    var hostCheckins = await HostService.GetCheckInsLatestByHostIdAsync(widget.Host.Id, 100);
                    await widget.SetCheckins(hostCheckins.Data.ToList());
                }
                else
                {
                    var latestCheckin = await HostService.GetCheckInsLatestByHostIdAsync(widget.Host.Id, 1);
                    await widget.UpdateState(latestCheckin.Data.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }

        await InvokeAsync(StateHasChanged);
    }
    
    private async Task RefreshData()
    {
        var response = await HostService.SearchPaginatedAsync(_searchText, _pageNumber, _pageSize);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _hostWidgets.Clear();
        _pagedData = response.Data;
        _totalItems = response.TotalCount;
        _totalPages = response.EndPage;
        
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
    
    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }
    
    public async ValueTask DisposeAsync()
    {
        _timer?.Dispose();
        await Task.CompletedTask;
    }
}