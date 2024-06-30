using System.Text;
using System.Web;
using Application.Constants;
using Application.Requests.Host;

namespace Application.Helpers;

public class ApiHelpers
{
    public static HostRegisterRequest? GetRequestFromUrl(string registerUrl)
    {
        var queryParameters = HttpUtility.ParseQueryString(new Uri(registerUrl).Query, Encoding.UTF8);
        var hostId = queryParameters.Get(HostConstants.QueryHostId);
        var registerKey = queryParameters.Get(HostConstants.QueryHostRegisterKey);

        var isValidGuid = Guid.TryParse(hostId, out var hostIdConverted);
        if (hostId is null || string.IsNullOrWhiteSpace(registerKey) || !isValidGuid)
        {
            return null;
        }

        return new HostRegisterRequest
        {
            HostId = hostIdConverted,
            Key = registerKey
        };
    }
}