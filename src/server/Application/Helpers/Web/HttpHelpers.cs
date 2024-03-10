using Microsoft.AspNetCore.Http;

namespace Application.Helpers.Web;

public static class HttpHelpers
{
    public static string GetConnectionIp(this HttpContext context)
    {
        // TODO: Enumerate headers configured in WebServerConfiguration
        // Check for the X-Forwarded-For header first
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = string.IsNullOrEmpty(forwardedHeader)
            ? context.Connection.RemoteIpAddress?.ToString()
            : forwardedHeader.Split(',').FirstOrDefault()?.Trim();

        return ip ?? "127.0.0.1";
    }
}