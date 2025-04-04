using System.Net;
using System.Net.Sockets;

namespace Application.Helpers.Runtime;

public static class NetworkHelpers
{
    public static List<int> GetPortsFromRangeList(IEnumerable<string>? portRanges)
    {
        var parsedPorts = new List<int>();

        if (portRanges is null)
        {
            return parsedPorts;
        }

        foreach (var portOrRange in portRanges)
        {
            try
            {
                if (portOrRange.Contains('-'))
                {
                    // Handle port range
                    var parts = portOrRange.Split('-');
                    if (parts.Length != 2 || !int.TryParse(parts[0], out var rangeStart) || !int.TryParse(parts[1], out var rangeEnd)) continue;

                    for (var port = rangeStart; port <= rangeEnd; port++)
                    {
                        parsedPorts.Add(port);
                    }
                }
                else
                {
                    // Handle single port
                    if (int.TryParse(portOrRange, out var port))
                    {
                        parsedPorts.Add(port);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore parse failures
            }
        }

        return parsedPorts;
    }

    public static async Task<string?> ParseIpFromIpOrHost(string ipOrHost)
    {
        if (IPAddress.TryParse(ipOrHost, out var ip))
        {
            return ip.ToString();
        }

        // The input is a hostname, perform DNS lookup
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(ipOrHost);
            foreach (var address in addresses)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return address.ToString();
                }
            }

            return addresses[0].ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }
}