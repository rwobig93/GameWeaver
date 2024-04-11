using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Domain.Contracts;

public class WebResult : IWebResult
{
    public IResult Result { get; set; } = null!;
    
    private int StatusCode { get; set; } = StatusCodes.Status200OK;


    public async Task ExecuteAsync(HttpContext? httpContext)
    {
        // if (httpContext is null) return;
        //
        // // Only modify the response if it hasn't been modified already
        // // if (httpContext.Response.HasStarted) return;
        //
        // httpContext.Response.ContentType = "application/json";
        // httpContext.Response.StatusCode = StatusCode;
        //
        // var response = JsonSerializer.Serialize(Result);
        // await httpContext.Response.WriteAsync(response);
        await Task.CompletedTask;
    }

    private static IWebResult ResultGen(IResult result, int statusCode = 0)
    {
        return new WebResult {Result = result, StatusCode = statusCode};
    }

    public static Task<IWebResult> ResultAsync(IResult result, int statusCode = 0)
    {
        if (statusCode == 0)
        {
            statusCode = result.Succeeded ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;
        }
        
        return Task.FromResult(ResultGen(result, statusCode));
    }

    public static Task<IWebResult> SuccessAsync(IResult result)
    {
        return Task.FromResult(ResultGen(result, StatusCodes.Status200OK));
    }

    public static Task<IWebResult> FailureAsync(IResult result)
    {
        return Task.FromResult(ResultGen(result, StatusCodes.Status500InternalServerError));
    }
}