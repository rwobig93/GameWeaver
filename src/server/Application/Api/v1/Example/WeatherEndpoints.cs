using Application.Constants.Web;
using Application.Helpers.Web;
using Application.Models.Web;
using Application.Responses.Example;
using Application.Services.Example;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.v1.Example;

/// <summary>
/// API endpoints for the example weather service
/// </summary>
public static class WeatherEndpoints
{
    /// <summary>
    /// Registers the example weather endpoints
    /// </summary>
    /// <param name="app"></param>
    public static void MapEndpointsWeather(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRouteConstants.Example.Weather, GetForecastAsync).ApiVersionOne();
    }

    /// <summary>
    /// Get randomly generated example weather data
    /// </summary>
    /// <param name="startDate">Inclusive date to start with for weather retrieval</param>
    /// <param name="weatherCount">Number of days to get weather data for</param>
    /// <param name="weatherForecast"></param>
    /// <returns>List of weather data for each day</returns>
    [AllowAnonymous]
    private static async Task<IResult<WeatherDataResponse[]>> GetForecastAsync([FromQuery]DateOnly? startDate, [FromQuery]int? weatherCount,
        IWeatherService  weatherForecast)
    {
        try
        {
            var startDateConverted = startDate ?? DateOnly.FromDateTime(DateTime.Now);
            var weatherCountConverted = weatherCount ?? 10;

            var weatherForecastData = await weatherForecast.GetForecastAsync(startDateConverted, weatherCountConverted);
            
            return await Result<WeatherDataResponse[]>.SuccessAsync(weatherForecastData);
        }
        catch (Exception ex)
        {
            return await Result<WeatherDataResponse[]>.FailAsync(ex.Message);
        }
    }
}