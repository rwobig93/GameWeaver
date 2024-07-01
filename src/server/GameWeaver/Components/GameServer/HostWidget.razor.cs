using Microsoft.AspNetCore.Components;

namespace GameWeaver.Components.GameServer;

public partial class HostWidget : ComponentBase, IAsyncDisposable
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;
    [Parameter] public string FriendlyName { get; set; } = "Local Machine For Testing";
    [Parameter] public string Hostname { get; set; } = "fortis-novacula";
    [Parameter] public string PublicIp { get; set; } = "72.46.27.138";
    private Timer? _timer;
    private double[] _cpuData = [0, Random.Shared.Next(20, 80)];
    private double[] _ramData = [0, Random.Shared.Next(30, 80)];
    private readonly List<ChartSeries> _netIn = [];
    private readonly List<ChartSeries> _netOut = [];
    private readonly ChartOptions _chartOptionsCompute = new()
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

    public string PrimaryColor { get; set; } = "";
    public string SurfaceColor { get; set; } = "";
    public string TertiaryColor { get; set; } = "";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // TODO: Add different color for CPU, RAM, Net In and Net Out
            PrimaryColor = ParentLayout._selectedTheme.Palette.Primary.Value;
            SurfaceColor = ParentLayout._selectedTheme.Palette.Surface.Value;
            TertiaryColor = ParentLayout._selectedTheme.Palette.Tertiary.Value;
            _chartOptionsCompute.ChartPalette = [SurfaceColor, PrimaryColor];
            _chartOptionsNetwork.ChartPalette = [TertiaryColor, SurfaceColor];
            _timer = new Timer(async _ =>
            {
                await UpdateDataNetwork();
                await UpdateData();
                
                PrimaryColor = ParentLayout._selectedTheme.Palette.Primary.Value;
                SurfaceColor = ParentLayout._selectedTheme.Palette.Surface.Value;
                TertiaryColor = ParentLayout._selectedTheme.Palette.Tertiary.Value;
                _chartOptionsCompute.ChartPalette = [SurfaceColor, PrimaryColor];
                _chartOptionsNetwork.ChartPalette = [TertiaryColor, SurfaceColor];
                await InvokeAsync(StateHasChanged);
            }, null, 0, 1000);
            await Task.CompletedTask;
            StateHasChanged();
        }
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
    }

    private async Task UpdateData()
    {
        var currentCpu = Random.Shared.Next(5, 99);
        var cpuLeftOver = 100 - currentCpu;

        _cpuData = [cpuLeftOver, currentCpu];

        var currentRam = Random.Shared.Next(30, 80);
        var ramLeftOver = 100 - currentRam;

        _ramData = [ramLeftOver, currentRam];
    }
    
    public async ValueTask DisposeAsync()
    {
        _timer?.Dispose();
        await Task.CompletedTask;
    }
}