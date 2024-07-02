
using Application.Constants.Communication;
using Application.Helpers.Runtime;

namespace GameWeaver.Components.GameServer;

public partial class HostWidget : ComponentBase, IAsyncDisposable
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;
    [Parameter] public string FriendlyName { get; set; } = "";
    [Parameter] public string Hostname { get; set; } = "fortis-novacula";
    [Parameter] public string PublicIp { get; set; } = "72.46.27.138";
    
    private Timer? _timer;
    private double[] _cpuData = [0, Random.Shared.Next(20, 80)];
    private double[] _ramData = [0, Random.Shared.Next(30, 80)];
    private double _storageUsed = 50;
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
    private DateTime Uptime { get; set; } = new(2024, 
        Random.Shared.Next(3, 6), 
        Random.Shared.Next(1, 30), 
        Random.Shared.Next(1, 24), 
        Random.Shared.Next(1, 60), 
        Random.Shared.Next(1, 60));
    private DateTime _wentOffline = DateTime.Now;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            UpdateThemeColors();
            UpdateStatus();

            if (string.IsNullOrWhiteSpace(FriendlyName))
            {
                FriendlyName = NameHelpers.GenerateHostname();
            }

            if (FriendlyName == "offline")
            {
                _wentOffline = DateTime.Now;
                StatusColor = Color.Error;
                IsOffline = true;
            }
            
            PublicIp = $"{Random.Shared.Next(193, 254)}.{Random.Shared.Next(0, 254)}.{Random.Shared.Next(0, 254)}.{Random.Shared.Next(1, 254)}";
            _storageUsed = Random.Shared.Next(20, 99);
            
            _timer = new Timer(async _ =>
            {
                UpdateThemeColors();

                if (FriendlyName != "offline")
                {
                    UpdateStatus();
                    await UpdateDataNetwork();
                    await UpdateData();
                }
                
                await InvokeAsync(StateHasChanged);
            }, null, 0, 1000);
            
            await Task.CompletedTask;
            StateHasChanged();
        }
    }

    private string GetUptime()
    {
        TimeSpan totalUpdate;
        if (IsOffline)
        {
            totalUpdate = _wentOffline - Uptime;
        }
        else
        {
            totalUpdate = DateTime.Now - Uptime;
        }
        return $"{totalUpdate.Days}d {totalUpdate.Hours}h {totalUpdate.Minutes}m {totalUpdate.Seconds}s";
    }

    private string GetDowntime()
    {
        var offlineTime = DateTime.Now - _wentOffline;
        return $"{offlineTime.Days}d {offlineTime.Hours}h {offlineTime.Minutes}m {offlineTime.Seconds}s";
    }

    private void UpdateStatus()
    {
        var testStatusValue = Random.Shared.Next(1, 11);

        switch (testStatusValue)
        {
            case <= 6:
                StatusColor = Color.Success;
                IsOffline = false;
                return;
            case 7 or 8:
                StatusColor = Color.Error;
                IsOffline = true;
                _wentOffline = DateTime.Now;
                break;
            default:
                StatusColor = Color.Warning;
                IsOffline = false;
                break;
        }
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
    
    private async Task UpdateDataNetwork()
    {
        List<double> newNetIn = [];
        List<double> newNetOut = [];

        for (var i = 0; i < 100; i++)
        {
            newNetIn.Add(Random.Shared.Next(0, 1_000_000));
            newNetOut.Add(Random.Shared.Next(0, 1_000_000));
        }
        
        _netIn.Clear();
        _netOut.Clear();
        
        _netIn.Add(new ChartSeries {Data = newNetIn.ToArray()});
        _netOut.Add(new ChartSeries {Data = newNetOut.ToArray()});
        await Task.CompletedTask;
    }

    private async Task UpdateData()
    {
        var currentCpu = Random.Shared.Next(5, 99);
        var cpuLeftOver = 100 - currentCpu;

        _cpuData = [cpuLeftOver, currentCpu];

        var currentRam = Random.Shared.Next(30, 80);
        var ramLeftOver = 100 - currentRam;

        _ramData = [ramLeftOver, currentRam];
        await Task.CompletedTask;
    }
    
    public async ValueTask DisposeAsync()
    {
        _timer?.Dispose();
        await Task.CompletedTask;
    }
}