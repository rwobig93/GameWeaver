using Application.Constants.Identity;
using Application.Helpers.Runtime;
using Application.Models.GameServer.Host;
using Application.Services.GameServer;
using Application.Settings.AppSettings;
using GameWeaver.Components.GameServer;
using Microsoft.Extensions.Options;

namespace GameWeaver.Pages.GameServer;

public partial class HostsDashboard : ComponentBase, IAsyncDisposable
{
    [Inject] private IHostService HostService { get; set; } = null!;
    [Inject] private IWebClientService WebClientService { get; set; } = null!;
    [Inject] private IOptions<AppConfiguration> AppConfig { get; init; } = null!;
    [Inject] public IOptions<LifecycleConfiguration> LifecycleConfig { get; init; } = null!;
    
    private IEnumerable<HostSlim> _pagedData = new List<HostSlim>();
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private Timer? _timer;
    private readonly List<HostWidget> _hostWidgets = [];
    public HostWidget WidgetReference
    {
        set => _hostWidgets.Add(value);
    }

    private readonly string _searchText = "";
    private int _totalItems = 10;
    private int _totalPages = 1;
    private int _pageSize = PaginationHelpers.GetPageSizes().First();
    private int _pageNumber;

    private bool _canCreateRegistrations;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await GetPermissions();
            await GetClientTimezone();
            await RefreshData();
            await TimerDataUpdate();
            
            _timer = new Timer(async _ => { await TimerDataUpdate(); }, null, 0, 1000);
            
            await Task.CompletedTask;
        }
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _canCreateRegistrations = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.HostRegistration.Create);
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

        if (_totalItems == 0)
        {
            currentStart = 0;
        }

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

    private async Task PageSizeChanged()
    {
        await RefreshData();
        await WebClientService.InvokeScrollToTop();
    }
    
    private async Task SearchKeyDown(KeyboardEventArgs keyArgs)
    {
        if (keyArgs.Code is "Enter" or "NumpadEnter")
        {
            await RefreshData();
        }
    }
    
    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }

    private async Task NewRegistration()
    {
        if (!_canCreateRegistrations)
        {
            return;
        }
        
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<HostRegisterDialog>("Generate New Host Registration", new DialogParameters(), dialogOptions);
        var dialogResult = await dialog.Result;
        if (dialogResult?.Data is null || dialogResult.Canceled)
        {
            return;
        }

        var newRegistrationUrl = (string) dialogResult.Data;
        if (string.IsNullOrWhiteSpace(newRegistrationUrl))
        {
            Snackbar.Add("Failed to retrieve the new host registration, please try again or reach out to an administrator", Severity.Error);
            return;
        }
        
        Snackbar.Add("Successfully generated new host registration!", Severity.Success);
        var copyParameters = new DialogParameters()
        {
            {"Title", $"Please copy this registration, if it isn't used it will expire after {LifecycleConfig.Value.HostRegistrationCleanupHours} hours"},
            {"FieldLabel", "New Host Registration"},
            {"TextToDisplay", newRegistrationUrl},
            {"TextToCopy", newRegistrationUrl}
        };
        await DialogService.ShowAsync<CopyTextDialog>("Copy New Host Registration", copyParameters, dialogOptions);
    }
    
    public async ValueTask DisposeAsync()
    {
        _timer?.Dispose();
        await Task.CompletedTask;
    }
}