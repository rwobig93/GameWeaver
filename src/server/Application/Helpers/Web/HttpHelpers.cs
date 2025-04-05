using Application.Constants.Web;
using Microsoft.AspNetCore.Http;

namespace Application.Helpers.Web;

public static class HttpHelpers
{
    public static string GetConnectionIp(this HttpContext context)
    {
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = string.IsNullOrEmpty(forwardedHeader)
            ? context.Connection.RemoteIpAddress?.ToString()
            : forwardedHeader.Split(',').FirstOrDefault()?.Trim();

        return ip ?? "127.0.0.1";
    }

    public static async Task<string> GetPublicIp(this IHttpClientFactory httpClientFactory)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(ApiConstants.Clients.GeneralWeb);
            var publicIpResponse = await httpClient.GetAsync(ApiConstants.GeneralExternal.UrlGetPublicIp);
            var parsedPublicIp = await publicIpResponse.Content.ReadAsStringAsync();
            return parsedPublicIp.Trim();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}