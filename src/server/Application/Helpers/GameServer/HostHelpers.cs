using Domain.Models.Host;

namespace Application.Helpers.GameServer;

public static class HostHelpers
{
    public static HostNetworkInterface? GetPrimaryInterface(this IEnumerable<HostNetworkInterface>? networkInterfaces)
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

        return interfaces
            .Where(netInterface => netInterface.IpAddresses.Count != 0)
            .FirstOrDefault(netInterface => !netInterface.IpAddresses.First().StartsWith("127."));
    }
    
    public static string? GetPrimaryIp(this IEnumerable<HostNetworkInterface>? networkInterfaces)
    {
        return networkInterfaces.GetPrimaryInterface()?.IpAddresses.First();
    }
}