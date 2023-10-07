using Application.Responses.v1.Example;
using Application.Services.Example;

namespace Infrastructure.Services.Example;

public class WeatherForecastService : IWeatherService
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public async Task<WeatherDataResponse[]> GetForecastAsync(DateOnly startDate, int count = 10)
    {
        if (count < 2) count = 2;
        return await Task.FromResult(Enumerable.Range(0, count).Select(index => new WeatherDataResponse
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray());
    }
}