using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Application.Models.Web;

public class Result : IResult
{
    public List<string> Messages { get; set; } = new ();

    public bool Succeeded { get; set; }

    private int StatusCode { get; set; } = StatusCodes.Status200OK;


    public async Task ExecuteAsync(HttpContext? httpContext)
    {
        if (httpContext is null) return;
        
        // Only modify the response if it hasn't been modified already
        if (httpContext.Response.HasStarted) return;

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = StatusCode;

        var response = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            {"Messages", Messages},
            {"Succeeded", Succeeded}
        });
        await httpContext.Response.WriteAsync(response);
    }
    
    public static IResult Fail(int statusCode = StatusCodes.Status500InternalServerError)
    {
        return new Result { Succeeded = false };
    }

    public static IResult Fail(string message, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return new Result { Succeeded = false, Messages = new List<string> { message }, StatusCode = statusCode };
    }

    public static IResult Fail(List<string> messages, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return new Result { Succeeded = false, Messages = messages, StatusCode = statusCode };
    }

    public static Task<IResult> FailAsync(int statusCode = StatusCodes.Status500InternalServerError)
    {
        return Task.FromResult(Fail(statusCode: statusCode));
    }

    public static Task<IResult> FailAsync(string message, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return Task.FromResult(Fail(message, statusCode: statusCode));
    }

    public static Task<IResult> FailAsync(List<string> messages, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return Task.FromResult(Fail(messages, statusCode: statusCode));
    }

    public static IResult Success(int statusCode = StatusCodes.Status200OK)
    {
        return new Result { Succeeded = true, StatusCode = statusCode };
    }

    public static IResult Success(string message, int statusCode = StatusCodes.Status200OK)
    {
        return new Result { Succeeded = true, Messages = new List<string> { message }, StatusCode = statusCode };
    }

    public static Task<IResult> SuccessAsync(int statusCode = StatusCodes.Status200OK)
    {
        return Task.FromResult(Success(statusCode: statusCode));
    }

    public static Task<IResult> SuccessAsync(string message, int statusCode = StatusCodes.Status200OK)
    {
        return Task.FromResult(Success(message, statusCode: statusCode));
    }
}

public class Result<T> : Result, IResult<T>
{
    public T Data { get; set; } = default!;


    public new async Task ExecuteAsync(HttpContext? httpContext)
    {
        if (httpContext is null) return;
        
        // Only modify the response if it hasn't been modified already
        if (httpContext.Response.HasStarted) return;

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = StatusCode;

        var response = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            {"Messages", Messages},
            {"Succeeded", Succeeded},
            {"Data", Data!}
        });
        await httpContext.Response.WriteAsync(response);
    }

    private int StatusCode { get; set; } = StatusCodes.Status200OK;

    public new static Result<T> Fail(int statusCode = StatusCodes.Status500InternalServerError)
    {
        return new Result<T> { Succeeded = false, StatusCode = statusCode };
    }

    public new static Result<T> Fail(string message, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return new Result<T> { Succeeded = false, Messages = new List<string> { message }, StatusCode = statusCode };
    }

    public new static Result<T> Fail(List<string> messages, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return new Result<T> { Succeeded = false, Messages = messages, StatusCode = statusCode };
    }

    public static Result<T> Fail(T data, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return new Result<T> { Succeeded = false, Data = data, StatusCode = statusCode };
    }

    public static Result<T> Fail(T data, string message, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return new Result<T> { Succeeded = false, Data = data, Messages = new List<string> { message }, StatusCode = statusCode };
    }

    public static Result<T> Fail(T data, List<string> messages, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return new Result<T> { Succeeded = false, Data = data, Messages = messages, StatusCode = statusCode };
    }

    public new static Task<Result<T>> FailAsync(int statusCode = StatusCodes.Status500InternalServerError)
    {
        return Task.FromResult(Fail(statusCode: statusCode));
    }

    public new static Task<Result<T>> FailAsync(string message, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return Task.FromResult(Fail(message, statusCode: statusCode));
    }

    public new static Task<Result<T>> FailAsync(List<string> messages, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return Task.FromResult(Fail(messages, statusCode: statusCode));
    }

    public static Task<Result<T>> FailAsync(T data, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return Task.FromResult(Fail(data, statusCode: statusCode));
    }

    public static Task<Result<T>> FailAsync(T data, string message, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return Task.FromResult(Fail(data, message, statusCode: statusCode));
    }

    public static Task<Result<T>> FailAsync(T data, List<string> messages, int statusCode = StatusCodes.Status500InternalServerError)
    {
        return Task.FromResult(Fail(data, messages, statusCode: statusCode));
    }

    public new static Result<T> Success(int statusCode = StatusCodes.Status200OK)
    {
        return new Result<T> { Succeeded = true, StatusCode = statusCode };
    }

    public static Result<T> Success(T data, int statusCode = StatusCodes.Status200OK)
    {
        return new Result<T> { Succeeded = true, Data = data, StatusCode = statusCode };
    }

    public static Result<T> Success(T data, string message, int statusCode = StatusCodes.Status200OK)
    {
        return new Result<T> { Succeeded = true, Data = data, Messages = new List<string> { message }, StatusCode = statusCode };
    }

    public static Result<T> Success(T data, List<string> messages, int statusCode = StatusCodes.Status200OK)
    {
        return new Result<T> { Succeeded = true, Data = data, Messages = messages, StatusCode = statusCode };
    }

    public new static Task<Result<T>> SuccessAsync(int statusCode = StatusCodes.Status200OK)
    {
        return Task.FromResult(Success(statusCode: statusCode));
    }

    public static Task<Result<T>> SuccessAsync(T data, int statusCode = StatusCodes.Status200OK)
    {
        return Task.FromResult(Success(data, statusCode: statusCode));
    }

    public static Task<Result<T>> SuccessAsync(T data, string message, int statusCode = StatusCodes.Status200OK)
    {
        return Task.FromResult(Success(data, message, statusCode: statusCode));
    }
}
