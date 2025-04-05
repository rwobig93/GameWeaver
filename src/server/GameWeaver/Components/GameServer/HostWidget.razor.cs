
using Application.Helpers.Runtime;
using Application.Models.GameServer.Host;
using Application.Models.GameServer.HostCheckIn;
using Application.Settings.AppSettings;
using Microsoft.Extensions.Options;

#pragma warning disable CS0618 // Type or member is obsolete

namespace GameWeaver.Components.GameServer;

public partial class HostWidget : ComponentBase
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;
    [Parameter] public HostSlim Host { get; set; } = null!;
    [Parameter] public TimeZoneInfo LocalTimeZone { get; set; } = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    [Inject] private IOptions<AppConfiguration> AppConfig { get; init; } = null!;


    private double[] _cpuData = [0, 0];
    private double[] _ramData = [0, 0];
    private readonly List<TimeSeriesChartSeries> _netIn = [];
    private readonly List<TimeSeriesChartSeries> _netOut = [];
    private readonly ChartOptions _chartOptionsCpu = new()
    {
        LineStrokeWidth = 4,
        InterpolationOption = InterpolationOption.NaturalSpline,
        YAxisLines = false,
        XAxisLines = false,
        ShowLegend = false,
        ShowToolTips = false,
        ShowLabels = false,
        ShowLegendLabels = false
    };
    private readonly ChartOptions _chartOptionsRam = new()
    {
        LineStrokeWidth = 4,
        InterpolationOption = InterpolationOption.NaturalSpline,
        YAxisLines = false,
        XAxisLines = false,
        ShowLegend = false,
        ShowToolTips = false,
        ShowLabels = false,
        ShowLegendLabels = false
    };
    private readonly ChartOptions _chartOptionsNetwork = new()
    {
        LineStrokeWidth = 2,
        YAxisLines = false,
        XAxisLines = false,
        ShowLegend = false,
        YAxisTicks = 500,
        YAxisRequireZeroPoint = true,
        YAxisLabelPosition = YAxisLabelPosition.None,
        XAxisLabelPosition = XAxisLabelPosition.None,
        ShowToolTips = false,
        ShowLabels = false,
        ShowLegendLabels = false
    };

    private List<HostCheckInFull> _checkins = [];
    private PaletteDark _currentPalette = new();
    private Color StatusColor { get; set; } = Color.Success;
    private bool IsOffline { get; set; }
    private DateTime WentOffline { get; set; }
    private double StorageUsed { get; set; } = 0;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            UpdateStorage();
            await UpdateState();
        }
    }

    public async Task SetCheckins(List<HostCheckInFull> checkins)
    {
        _checkins = checkins;
        _checkins.Reverse();
        await InvokeAsync(StateHasChanged);
    }

    public int GetCheckinCount()
    {
        return _checkins.Count;
    }

    public async Task UpdateState(HostCheckInFull? latestCheckin = null)
    {
        if (latestCheckin is not null)
        {
            _checkins.Add(latestCheckin);
            if (_checkins.Count != 0)
            {
                _checkins.Remove(_checkins.First());
            }
        }

        UpdateThemeColors();
        UpdateStatus();

        if (!IsOffline)
        {
            await UpdateNetwork();
            await UpdateCompute();
        }

        await InvokeAsync(StateHasChanged);
    }

    private string GetUptime()
    {
        if (_checkins.Count == 0)
        {
            return "0d 0h 0m 0s";
        }

        var hostUptime = TimeSpan.FromMilliseconds(_checkins.LastOrDefault()?.Uptime ?? 0);
        return $"{hostUptime.Days}d {hostUptime.Hours}h {hostUptime.Minutes}m {hostUptime.Seconds}s";
    }

    private string GetDowntime()
    {
        if (_checkins.Count == 0)
        {
            return "0d 0h 0m 0s";
        }

        WentOffline = _checkins.LastOrDefault()?.ReceiveTimestamp ?? DateTimeService.NowDatabaseTime;
        var offlineTime = DateTimeService.NowDatabaseTime - WentOffline;
        return $"{offlineTime.Days}d {offlineTime.Hours}h {offlineTime.Minutes}m {offlineTime.Seconds}s";
    }

    private void UpdateStatus()
    {
        if (_checkins.Count == 0)
        {
            IsOffline = false;
            StatusColor = Color.Warning;
            return;
        }

        var lastCheckinTime = _checkins.Last().ReceiveTimestamp;
        var currentTime = DateTimeService.NowDatabaseTime;
        if ((currentTime - lastCheckinTime).TotalSeconds > AppConfig.Value.HostOfflineAfterSeconds)
        {
            IsOffline = true;
            StatusColor = Color.Error;
            return;
        }

        // When ready we can set the status color for host warnings (disk space, resource usage, ect)
        // StatusColor = Color.Warning;
        IsOffline = false;
        StatusColor = Color.Success;
    }

    private void UpdateThemeColors()
    {
        if (_currentPalette == ParentLayout._selectedTheme.PaletteDark)
        {
            return;
        }

        _currentPalette = ParentLayout._selectedTheme.PaletteDark;

        _chartOptionsCpu.ChartPalette = [_currentPalette.Surface.Value, _currentPalette.Primary.Value];
        _chartOptionsRam.ChartPalette = [_currentPalette.Surface.Value, _currentPalette.Secondary.Value];
        _chartOptionsNetwork.ChartPalette = [_currentPalette.Tertiary.Value, _currentPalette.Surface.Value];
    }

    private async Task UpdateCompute()
    {
        if (_checkins.Count == 0)
        {
            return;
        }

        var currentCpu = (double) (_checkins.LastOrDefault()?.CpuUsage ?? 0);
        var cpuLeftOver = 100 - currentCpu;

        _cpuData = [cpuLeftOver, currentCpu];

        var currentRam = (int) (_checkins.LastOrDefault()?.RamUsage ?? 0);
        var ramLeftOver = 100 - currentRam;

        _ramData = [ramLeftOver, currentRam];
        await Task.CompletedTask;
    }

    private async Task UpdateNetwork()
    {
        if (_checkins.Count == 0)
        {
            return;
        }

        _netIn.Clear();
        _netOut.Clear();

        var networkIn = _checkins.Select(x =>
            new TimeSeriesChartSeries.TimeValue(x.ReceiveTimestamp.ConvertToLocal(LocalTimeZone), Math.Ceiling((double)x.NetworkInBytes / 8_000))).Reverse().ToList();
        var networkOut = _checkins.Select(x =>
            new TimeSeriesChartSeries.TimeValue(x.ReceiveTimestamp.ConvertToLocal(LocalTimeZone), Math.Ceiling((double)x.NetworkOutBytes / 8_000))).Reverse().ToList();

        _netIn.Add(new TimeSeriesChartSeries { Data = networkIn });
        _netOut.Add(new TimeSeriesChartSeries { Data = networkOut });
        await Task.CompletedTask;
    }

    private void UpdateStorage()
    {
        var freeSpace = Host.Storage?.Sum(x => (double) x.FreeSpace) ?? 0;
        var totalSpace = Host.Storage?.Sum(x => (double) x.TotalSpace) ?? 0;

        StorageUsed = 100 - Math.Round(freeSpace / totalSpace * 100);
        StateHasChanged();
    }

    private void ViewHost()
    {
        NavManager.NavigateTo(AppRouteConstants.GameServer.Hosts.ViewId(Host.Id));
    }
}