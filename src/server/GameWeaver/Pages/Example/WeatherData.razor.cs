using Application.Responses.v1.Example;

namespace GameWeaver.Pages.Example;

public partial class WeatherData
{
    [CascadingParameter] public MainLayout ParentLayout { get; set; } = null!;

    private bool _dense = true;
    private bool _hover = true;
    private bool _striped = true;
    private bool _bordered;
    private int _daysToGet = 100;
    private string _searchString = "";
    private WeatherDataResponse _selectedItem = null!;
    
    private WeatherDataResponse[]? _forecasts;

    protected override async Task OnInitializedAsync()
    {
        await RefreshWeatherData();
    }

    private bool SearchFunction(WeatherDataResponse weatherResponse)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;
        if ($"{weatherResponse.Date} {weatherResponse.Summary}".Contains(_searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        return $"{weatherResponse.TemperatureC} {weatherResponse.TemperatureF}".Contains(_searchString, 
            StringComparison.OrdinalIgnoreCase);
    }

    private async Task RefreshWeatherData()
    {
        _forecasts = await WeatherForecast.GetForecastAsync(DateOnly.FromDateTime(DateTime.Now), _daysToGet);
    }
}