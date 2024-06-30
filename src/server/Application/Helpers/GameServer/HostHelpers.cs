using Domain.Models.Host;

namespace Application.Helpers.GameServer;

public static class HostHelpers
{
    public static string? GetPrimaryIp(this IEnumerable<HostNetworkInterface>? networkInterfaces)
    {
        if (networkInterfaces is null)
        {
            return null;
        }

        var interfaces = networkInterfaces as HostNetworkInterface[] ?? networkInterfaces.ToArray();
        if (interfaces.Length == 0)
        {
            return null;
        }

        return interfaces.Where(netInterface => netInterface.IpAddresses.Count != 0)
            .SelectMany(netInterface => netInterface.IpAddresses.Where(address => !address.StartsWith("127."))).FirstOrDefault();
    }
}