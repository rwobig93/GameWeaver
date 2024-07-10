
using Application.Models.GameServer.Host;
using Application.Models.GameServer.HostCheckIn;
using Domain.Enums.GameServer;

namespace GameWeaver.Components.GameServer;

public partial class HostWidget : ComponentBase
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;
    [Parameter] public HostSlim Host { get; set; } = null!;
    [Parameter] public List<HostCheckInFull> Checkins { get; set; } = [];

    [Parameter] public TimeZoneInfo LocalTimeZone { get; set; } = TimeZoneInfo.FindSystemTimeZoneById("GMT");
    
    [Inject] public IDateTimeService DateTime { get; set; } = null!;
    
    private double[] _cpuData = [0, Random.Shared.Next(20, 80)];
    private double[] _ramData = [0, Random.Shared.Next(30, 80)];
    private readonly List<ChartSeries> _netIn = [];
    private readonly List<ChartSeries> _netOut = [];
    private readonly ChartOptions _chartOptionsCpu = new()
    {
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
        YAxisTicks = 1_000_000,
        YAxisLines = false,
        XAxisLines = false,
        DisableLegend = true
    };

    private MudTheme _currentTheme = new();
    private string PrimaryColor { get; set; } = "";
    private string SecondaryColor { get; set; } = "";
    private string TertiaryColor { get; set; } = "";
    private string SurfaceColor { get; set; } = "";
    private string ActiveColor { get; set; } = "";
    private string OfflineColor { get; set; } = "";
    private string WarningColor { get; set; } = "";
    private Color StatusColor { get; set; } = Color.Success;
    private bool IsOffline { get; set; }
    private DateTime WentOffline { get; set; }
    private double StorageUsed { get; set; } = 0;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            UpdateThemeColors();
            UpdateStatus();
            UpdateStorage();
            
            await Task.CompletedTask;
            StateHasChanged();
        }
    }

    public async Task UpdateState()
    {
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
        var hostUptime = TimeSpan.FromMilliseconds(Checkins.LastOrDefault()?.Uptime ?? 0);
        return $"{hostUptime.Days}d {hostUptime.Hours}h {hostUptime.Minutes}m {hostUptime.Seconds}s";
    }

    private string GetDowntime()
    {
        WentOffline = Checkins.LastOrDefault()?.ReceiveTimestamp ?? DateTime.NowDatabaseTime;
        var offlineTime = DateTime.NowDatabaseTime - WentOffline;
        return $"{offlineTime.Days}d {offlineTime.Hours}h {offlineTime.Minutes}m {offlineTime.Seconds}s";
    }

    private void UpdateStatus()
    {
        if (Host.CurrentState == ConnectivityState.Unknown)
        {
            IsOffline = true;
            StatusColor = Color.Error;
            return;
        }
        
        // When ready we can set the status color for host warnings (disk space, resource usage, ect) 
        // StatusColor = Color.Warning;
        StatusColor = Color.Success;
    }

    private void UpdateThemeColors()
    {
        if (_currentTheme == ParentLayout._selectedTheme)
        {
            return;
        }
        
        _currentTheme = ParentLayout._selectedTheme;
        PrimaryColor = _currentTheme.Palette.Primary.Value;
        SecondaryColor = _currentTheme.Palette.Secondary.Value;
        SurfaceColor = _currentTheme.Palette.Surface.Value;
        TertiaryColor = _currentTheme.Palette.Tertiary.Value;
        ActiveColor = _currentTheme.Palette.Success.Value;
        OfflineColor = _currentTheme.Palette.Error.Value;
        WarningColor = _currentTheme.Palette.Warning.Value;
        
        _chartOptionsCpu.ChartPalette = [SurfaceColor, PrimaryColor];
        _chartOptionsRam.ChartPalette = [SurfaceColor, SecondaryColor];
        _chartOptionsNetwork.ChartPalette = [TertiaryColor, SurfaceColor];
    }

    private async Task UpdateCompute()
    {
        var currentCpu = (double) (Checkins.LastOrDefault()?.CpuUsage ?? 0);
        var cpuLeftOver = 100 - currentCpu;

        _cpuData = [cpuLeftOver, currentCpu];

        var currentRam = (double) (Checkins.LastOrDefault()?.RamUsage ?? 0);
        var ramLeftOver = 100 - currentRam;

        _ramData = [ramLeftOver, currentRam];
        await Task.CompletedTask;
    }
    
    private async Task UpdateNetwork()
    {
        _netIn.Clear();
        _netOut.Clear();
        
        _netIn.Add(new ChartSeries { Data = Checkins.Select(x => (double) x.NetworkInBytes / 8_000).Reverse().ToArray() });
        _netOut.Add(new ChartSeries { Data = Checkins.Select(x => (double) x.NetworkOutBytes / 8_000).Reverse().ToArray() });
        await Task.CompletedTask;
    }

    private void UpdateStorage()
    {
        var freeSpace = Host.Storage?.Sum(x => (double)x.FreeSpace) ?? 0;
        var totalSpace = Host.Storage?.Sum(x => (double) x.TotalSpace) ?? 0;
        
        StorageUsed = Math.Round(totalSpace / freeSpace);
    }
}