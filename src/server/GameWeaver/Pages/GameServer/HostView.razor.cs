using Application.Constants.Communication;
using Application.Constants.Identity;
using Application.Helpers.GameServer;
using Application.Helpers.Runtime;
using Application.Mappers.GameServer;
using Application.Mappers.Identity;
using Application.Models.GameServer.GameServer;
using Application.Models.GameServer.Host;
using Application.Models.GameServer.HostCheckIn;
using Application.Responses.v1.Identity;
using Application.Services.GameServer;
using Domain.Contracts;
using GameWeaver.Components.GameServer;

#pragma warning disable CS0618 // Type or member is obsolete

namespace GameWeaver.Pages.GameServer;

public partial class HostView : ComponentBase, IAsyncDisposable
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;
    [Parameter] public Guid HostId { get; set; } = Guid.Empty;

    [Inject] public IHostService HostService { get; init; } = null!;
    [Inject] public IGameServerService GameServerService { get; init; } = null!;
    [Inject] public IAppUserService AppUserService { get; init; } = null!;
    [Inject] public IWebClientService WebClientService { get; init; } = null!;

    private bool _validIdProvided = true;
    private Guid _loggedInUserId = Guid.Empty;
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    private HostSlim _host = new() { Id = Guid.Empty };
    private UserBasicResponse _hostOwner = new() { Username = "Unknown" };
    private bool IsOffline { get; set; }
    private DateTime WentOffline { get; set; }
    private bool _editMode;
    private string _editButtonText = "Enable Edit Mode";
    private double _storageUsedTotal;
    private DateTime _checkinsAfterDate = DateTime.Now.AddMinutes(-2);
    private string _checkinsDateSelection = "2 Minutes";
    private Timer? _timer;
    private List<GameServerSlim> _runningGameservers = [];
    private List<HostCheckInFull> _checkins = [];
    private Palette _currentPalette = new();
    private double[] _cpuData = [0, 0];
    private double[] _ramData = [0, 0];
    private readonly List<ChartSeries> _netInShort = [];
    private readonly List<ChartSeries> _netInLong = [];
    private readonly List<ChartSeries> _netOutShort = [];
    private readonly List<ChartSeries> _netOutLong = [];
    private readonly List<ChartSeries> _cpu = [];
    private readonly List<ChartSeries> _ram = [];
    private readonly ChartOptions _chartOptionsCpu = new()
    {
        MaxNumYAxisTicks = 100,
        LineStrokeWidth = 4,
        InterpolationOption = InterpolationOption.NaturalSpline,
        YAxisLines = false,
        XAxisLines = false,
        DisableLegend = true
    };
    private readonly ChartOptions _chartOptionsRam = new()
    {
        LineStrokeWidth = 4,
        InterpolationOption = InterpolationOption.NaturalSpline,
        YAxisLines = false,
        XAxisLines = false,
        DisableLegend = true
    };
    private readonly ChartOptions _chartOptionsNetwork = new()
    {
        LineStrokeWidth = 4,
        InterpolationOption = InterpolationOption.NaturalSpline,
        YAxisLines = false,
        XAxisLines = false,
        DisableLegend = true,
    };
    private readonly List<string> _resourceHistoryChoices =
    [
        "2 Minutes",
        "10 Minutes",
        "30 Minutes",
        "1 Hour",
        "12 Hours",
        "1 Day",
        "7 Days",
        "14 Days",
        "30 Days"
    ];

    private bool _canEditHost;
    private bool _canViewGameServers;
    private bool _canDeleteHost;
    private bool _canChangeOwnership;
    
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                _checkinsDateSelection = _resourceHistoryChoices.First();
                await GetPermissions();
                await GetClientTimezone();
                await GetViewingHost();
                await GetHostOwner();
                await GetGameServers();
            
                _timer = new Timer(async _ => { await TimerDataUpdate(); }, null, 0, 1000);
                
                StateHasChanged();
            }
        }
        catch
        {
            StateHasChanged();
        }
    }

    private async Task TimerDataUpdate()
    {
        await UpdateCheckins();
        UpdateThemeColors();
        UpdateStatus();

        if (!IsOffline)
        {
            UpdateNetwork();
            UpdateCompute();
        }
        
        await InvokeAsync(StateHasChanged);
    }

    private async Task GetClientTimezone()
    {
        var clientTimezoneRequest = await WebClientService.GetClientTimezone();
        if (!clientTimezoneRequest.Succeeded)
        {
            clientTimezoneRequest.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clientTimezoneRequest.Data);
    }

    private async Task GetViewingHost()
    {
        var response = await HostService.GetByIdAsync(HostId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        if (response.Data is null)
        {
            Snackbar.Add(ErrorMessageConstants.Hosts.NotFound);
            _validIdProvided = false;
            StateHasChanged();
            return;
        }

        _host = response.Data;
        
        if (_host.Id == Guid.Empty)
        {
            _validIdProvided = false;
            StateHasChanged();
        }
    }

    private async Task GetHostOwner()
    {
        var response = await AppUserService.GetByIdAsync(_host.OwnerId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _hostOwner = response.Data?.ToResponse() ?? new UserBasicResponse { Username = "Unknown" };
    }

    private async Task GetGameServers()
    {
        if (!_canViewGameServers)
        {
            return;
        }

        _runningGameservers = [];
        var response = await GameServerService.GetByHostIdAsync(_host.Id);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        _runningGameservers = response.Data.ToList();
    }

    private async Task GetPermissions()
    {
        var currentUser = (await CurrentUserService.GetCurrentUserPrincipal())!;
        _loggedInUserId = CurrentUserService.GetIdFromPrincipal(currentUser);
        _canEditHost = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Hosts.Update);
        _canViewGameServers = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Gameserver.Get);
        _canDeleteHost = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Hosts.Delete) || _host.OwnerId == _loggedInUserId;
        _canChangeOwnership = await AuthorizationService.UserHasPermission(currentUser, PermissionConstants.GameServer.Hosts.ChangeOwnership) || _host.OwnerId == _loggedInUserId;
    }
    
    private async Task Save()
    {
        if (!_canEditHost) return;
        
        var response = await HostService.UpdateAsync(_host.ToUpdateRequest(), _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }
        
        ToggleEditMode();
        await GetViewingHost();
        Snackbar.Add("Host successfully updated!", Severity.Success);
        StateHasChanged();
    }

    private async Task ChangeOwnership()
    {
        if (!_canChangeOwnership)
        {
            return;
        }
        
        var dialogParameters = new DialogParameters()
        {
            {"ConfirmButtonText", "Change Host Owner"},
            {"Title", "Transfer Host Ownership"},
            {"OwnerId", _host.OwnerId}
        };
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };

        var dialogResult = await DialogService.Show<ChangeOwnershipDialog>("Transfer Host Ownership", dialogParameters, dialogOptions).Result;
        if (dialogResult.Canceled)
        {
            return;
        }

        var responseOwnerId = (Guid) dialogResult.Data;
        if (_host.OwnerId == responseOwnerId)
        {
            Snackbar.Add("Selected owner is already the owner, everything is as it was", Severity.Info);
            return;
        }

        var updateRequest = _host.ToUpdateRequest();
        updateRequest.OwnerId = responseOwnerId;

        var response = await HostService.UpdateAsync(updateRequest, _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Successfully transferred ownership!", Severity.Success);
        GoBack();
    }

    private async Task ChangeAllowedPorts()
    {
        if (!_canEditHost)
        {
            return;
        }
        
        var dialogParameters = new DialogParameters()
        {
            {"Title", $"Allowed Host Ports for {_host.FriendlyName}"},
            {"FieldLabel", "Allowed Host Ports"},
            {"ConfirmButtonText", "Change Ports"}
        };
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };

        var dialogResult = await DialogService.Show<ValuePromptDialog>("Change Allowed Host Ports", dialogParameters, dialogOptions).Result;
        if (dialogResult.Canceled)
        {
            return;
        }

        var allowedPortsRaw = (string) dialogResult.Data;
        var convertedPorts = allowedPortsRaw.Split(",").ToList();
        var parsedPorts = NetworkHelpers.GetPortsFromRangeList(convertedPorts);
        if (parsedPorts.Count < 3)
        {
            Snackbar.Add("Allowed ports provided doesn't have enough valid ports (3), please try again", Severity.Error);
            return;
        }

        _host.AllowedPorts = convertedPorts;
        Snackbar.Add("Provided ports are valid, please save changes to apply", Severity.Info);
        StateHasChanged();
    }

    private async Task DeleteHost()
    {
        if (!_canDeleteHost)
        {
            return;
        }
        
        var dialogParameters = new DialogParameters()
        {
            {"Title", "Are you sure you want to delete this host?"},
            {"Content", $"Host Name: {_host.FriendlyName}"}
        };
        var dialogOptions = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Large, CloseOnEscapeKey = true };

        var dialog = await DialogService.Show<ConfirmationDialog>("Delete Host", dialogParameters, dialogOptions).Result;
        if (dialog.Canceled)
        {
            return;
        }

        var response = await HostService.DeleteAsync(_host.Id, _loggedInUserId);
        if (!response.Succeeded)
        {
            response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
            return;
        }

        Snackbar.Add("Successfully deleted host!", Severity.Success);
        GoBack();
    }

    private void ToggleEditMode()
    {
        _editMode = !_editMode;

        _editButtonText = _editMode ? "Disable Edit Mode" : "Enable Edit Mode";
    }

    private void GoBack()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.Hosts.HostsDashboard);
    }

    private string CpuDisplay()
    {
        var cpuCount = _host.Cpus?.Count ?? 0;
        var firstCpu = _host.Cpus?.FirstOrDefault();
        var cpuName = firstCpu?.Name ?? "Unknown";
        var physicalCores = _host.Cpus?.Sum(x => x.CoreCount) ?? 0;
        var logicalCores = _host.Cpus?.Sum(x => x.LogicalProcessorCount) ?? 0;

        return $"{cpuCount}x {cpuName} w/ {physicalCores}physical & {logicalCores}logical";
    }

    private string GpuDisplay()
    {
        var gpuCount = _host.Gpus?.Count ?? 0;
        var firstGpu = _host.Gpus?.FirstOrDefault();
        var gpuName = firstGpu?.Name ?? "Unknown";
        var gpuRam = firstGpu?.AdapterRam ?? 0;

        return $"{gpuCount}x {gpuName} @ {gpuRam} VRAM";
    }

    private string MotherboardDisplay()
    {
        var motherboardCount = _host.Motherboards?.Count ?? 0;
        var firstMotherboard = _host.Motherboards?.FirstOrDefault();
        var motherboardManufacturer = firstMotherboard?.Manufacturer ?? "Generic";
        var motherboardProduct = firstMotherboard?.Product ?? "Unknown";

        return $"{motherboardCount}x {motherboardManufacturer} {motherboardProduct}";
    }

    private string RamDisplay()
    {
        var ramStickCount = _host.RamModules?.Count ?? 0;
        var firstStick = _host.RamModules?.FirstOrDefault();
        var ramTotal = _host.RamModules?.Sum(x => (double)x.Capacity) ?? 0;
        var firstStickSpeed = firstStick?.Speed ?? 0;

        return $"{ramStickCount}x {ramTotal} @ {firstStickSpeed}Mhz";
    }

    private string NetInterfaceDisplay()
    {
        var interfaceCount = _host.NetworkInterfaces?.Count ?? 0;
        var primaryInterface = _host.NetworkInterfaces.GetPrimaryInterface();
        var interfaceName = primaryInterface?.Name ?? "Unknown";
        var interfaceType = primaryInterface?.Type ?? "Generic";
        var interfaceAddress = primaryInterface?.IpAddresses.FirstOrDefault(x => !x.StartsWith("127.")) ?? "0.0.0.0";

        return $"{interfaceCount}x [{interfaceType}]{interfaceName} @ {interfaceAddress}";
    }

    private string StorageDisplay()
    {
        var storageCount = _host.Storage?.Count ?? 0;
        var firstStorage = _host.Storage?.FirstOrDefault()?.Model ?? "Unknown";
        var freeSpace = _host.Storage?.Sum(x => (double) x.FreeSpace) ?? 0;
        var totalSpace = _host.Storage?.Sum(x => (double) x.TotalSpace) ?? 0;
        _storageUsedTotal = 100 - Math.Round(freeSpace / totalSpace * 100);

        return $"{storageCount}x {firstStorage} @ {_storageUsedTotal}% Used";
    }

    private string UptimeDisplay()
    {
        if (_checkins.Count == 0)
        {
            return "0d 0h 0m 0s";
        }

        var hostUptime = TimeSpan.FromMilliseconds(_checkins.LastOrDefault()?.Uptime ?? 0);
        return $"{hostUptime.Days}d {hostUptime.Hours}h {hostUptime.Minutes}m {hostUptime.Seconds}s";
    }

    private async Task UpdateCheckins()
    {
        IResult<IEnumerable<HostCheckInFull>> response;
        switch (_checkins.Count)
        {
            case var count when count != 0:
                response = await HostService.GetCheckInsLatestByHostIdAsync(_host.Id, 1);
                if (!response.Succeeded)
                {
                    response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return;
                }
                
                _checkins.Add(response.Data.First());
                if (_checkins.Count != 0)
                {
                    _checkins.Remove(_checkins.First());   
                }
                break;
            default:
                response = await HostService.GetCheckInsAfterHostIdAsync(_host.Id, _checkinsAfterDate);
                if (!response.Succeeded)
                {
                    response.Messages.ForEach(x => Snackbar.Add(x, Severity.Error));
                    return;
                }
                
                _checkins = response.Data.ToList();
                _checkins.Reverse();
                break;
        }
    }

    private string DowntimeDisplay()
    {
        if (_checkins.Count == 0)
        {
            return "0d 0h 0m 0s";
        }

        WentOffline = _checkins.LastOrDefault()?.ReceiveTimestamp ?? DateTimeService.NowDatabaseTime;
        var offlineTime = DateTimeService.NowDatabaseTime - WentOffline;
        return $"{offlineTime.Days}d {offlineTime.Hours}h {offlineTime.Minutes}m {offlineTime.Seconds}s";
    }

    private string AllowedPortCountDisplay()
    {
        var portCount = NetworkHelpers.GetPortsFromRangeList(_host.AllowedPorts).Count;
        var gameServerCount = portCount / 3;

        return $"{portCount} total ports / {gameServerCount} game servers worth";
    }

    private void UpdateStatus()
    {
        if (_checkins.Count == 0)
        {
            IsOffline = false;
            return;
        }
        
        var lastCheckinTime = _checkins.Last().ReceiveTimestamp;
        var currentTime = DateTimeService.NowDatabaseTime;
        if ((currentTime - lastCheckinTime).TotalSeconds > 3)
        {
            IsOffline = true;
            return;
        }
        
        IsOffline = false;
    }

    private void UpdateThemeColors()
    {
        if (_currentPalette == ParentLayout._selectedTheme.Palette)
        {
            return;
        }
        
        _currentPalette = ParentLayout._selectedTheme.Palette;
        
        _chartOptionsCpu.ChartPalette = [_currentPalette.Surface.Value, _currentPalette.Primary.Value];
        _chartOptionsRam.ChartPalette = [_currentPalette.Surface.Value, _currentPalette.Secondary.Value];
        _chartOptionsNetwork.ChartPalette = [_currentPalette.Tertiary.Value, _currentPalette.Surface.Value];
    }

    private void UpdateCompute()
    {
        if (_checkins.Count == 0)
        {
            return;
        }

        var currentCpu = (double) (_checkins.LastOrDefault()?.CpuUsage ?? 0);
        var cpuLeftOver = 100 - currentCpu;

        _cpuData = [cpuLeftOver, currentCpu];
        _cpu.Clear();
        _cpu.Add(new ChartSeries { Data = _checkins.Select(x => Math.Round(x.CpuUsage, 0)).Reverse().ToArray() });

        var currentRam = (int) (_checkins.LastOrDefault()?.RamUsage ?? 0);
        var ramLeftOver = 100 - currentRam;

        _ramData = [ramLeftOver, currentRam];
        _ram.Clear();
        _ram.Add(new ChartSeries { Data = _checkins.Select(x => Math.Round(x.RamUsage, 0)).Reverse().ToArray() });
    }
    
    private void UpdateNetwork()
    {
        if (_checkins.Count == 0)
        {
            return;
        }
        
        _netInShort.Clear();
        _netInLong.Clear();
        _netOutShort.Clear();
        _netOutLong.Clear();
        
        _netInShort.Add(new ChartSeries { Data = _checkins.Select(x => (double) x.NetworkInBytes / 8_000).Take(100).Reverse().ToArray() });
        _netInLong.Add(new ChartSeries { Data = _checkins.Select(x => (double) x.NetworkInBytes / 8_000).Reverse().ToArray() });
        _netOutShort.Add(new ChartSeries { Data = _checkins.Select(x => (double) x.NetworkOutBytes / 8_000).Take(100).Reverse().ToArray() });
        _netOutLong.Add(new ChartSeries { Data = _checkins.Select(x => (double) x.NetworkOutBytes / 8_000).Reverse().ToArray() });
    }

    private async Task TimeframeChanged()
    {
        _checkinsAfterDate = _checkinsDateSelection switch
        {
            "10 Minutes" => DateTimeService.NowDatabaseTime.AddMinutes(-10),
            "30 Minutes" => DateTimeService.NowDatabaseTime.AddMinutes(-30),
            "1 Hour" => DateTimeService.NowDatabaseTime.AddHours(-1),
            "12 Hours" => DateTimeService.NowDatabaseTime.AddHours(-12),
            "1 Day" => DateTimeService.NowDatabaseTime.AddDays(-1),
            "7 Days" => DateTimeService.NowDatabaseTime.AddDays(-7),
            "14 Days" => DateTimeService.NowDatabaseTime.AddDays(-14),
            "30 Days" => DateTimeService.NowDatabaseTime.AddDays(-30),
            _ => DateTimeService.NowDatabaseTime.AddMinutes(-2)
        };

        _checkins = [];
        await UpdateCheckins();
        StateHasChanged();
    }
    
    public async ValueTask DisposeAsync()
    {
        _timer?.Dispose();
        await Task.CompletedTask;
    }
}