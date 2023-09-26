using Application.Responses.Example;

namespace Application.Services.Example;

public interface IWeatherService
{
    Task<WeatherDataResponse[]> GetForecastAsync(DateOnly startDate, int count = 10);
}