using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using Application.Constants.Communication;
using Domain.Contracts;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Middleware;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            // If the response has already been set then the error has been handled / communicated so no further action is needed
            if (context.Response.HasStarted) return;

            var response = context.Response;
            response.ContentType = "application/json";
            // TODO: Configure error message based on environment - development should be error.Message and prod should be Generic internal error message
            var errorMessage = error.Message;
            
            switch (error)
            {
                case not null when error.InnerException is JsonException jsonException:
                    response.StatusCode = (int) HttpStatusCode.BadRequest;
                    errorMessage = $"Invalid JSON value provided. Property: {jsonException.Path} | Line: {jsonException.LineNumber} | BytePosition: {jsonException.BytePositionInLine}";
                    break;
                case ApiException:
                case BadHttpRequestException:
                    response.StatusCode = (int) HttpStatusCode.BadRequest;
                    errorMessage = ErrorMessageConstants.Generic.InvalidValueError;
                    break;
                case SecurityTokenExpiredException:
                    response.StatusCode = (int) HttpStatusCode.Unauthorized;
                    errorMessage = ErrorMessageConstants.Authentication.TokenExpiredError;
                    break;
                case AuthenticationException:
                case SecurityTokenException:
                    response.StatusCode = (int) HttpStatusCode.Forbidden;
                    break;
                case KeyNotFoundException:
                    response.StatusCode = (int) HttpStatusCode.NotFound;
                    break;
                default:
                    response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    _logger.Error("Error occurred and handled by the middleware: {ErrorMessage}", errorMessage);
                    break;
            }
            
            var responseModel = await Result<string>.FailAsync(errorMessage);

            var result = JsonSerializer.Serialize(responseModel);
            await response.WriteAsync(result);
        }
    }
}