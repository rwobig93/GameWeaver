using System.Web;
using Application.Constants;
using Application.Requests.Host;

namespace Application.Helpers;

public class ApiHelpers
{
    public static HostRegisterRequest? GetRequestFromUrl(string registerUrl)
    {
        // TODO: HostId isn't being parsed for some reason, need to dig in further
        var hostId = HttpUtility.ParseQueryString(registerUrl).Get(HostConstants.QueryHostId);
        var registerKey = HttpUtility.ParseQueryString(registerUrl).Get(HostConstants.QueryHostRegisterKey);

        var isValidGuid = Guid.TryParse(hostId, out var hostIdConverted);
        if (hostId is null || string.IsNullOrWhiteSpace(registerKey) || !isValidGuid)
            return null;

        return new HostRegisterRequest
        {
            HostId = hostIdConverted,
            Key = registerKey
        };
    }
}