using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Application.Helpers;

public static class OsHelper
{
    public static string GetServiceRootPath()
    {
        return Directory.GetCurrentDirectory();
    }

    public static OSPlatform GetCurrentOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return OSPlatform.Windows;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OSPlatform.Linux;
        
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatform.OSX : OSPlatform.FreeBSD;
    }

    public static string GetConfigPath()
    {
        return Path.Combine(GetServiceRootPath(), "Config");
    }

    public static string GetDefaultGameServerPath()
    {
        return Path.Combine(GetServiceRootPath(), "GameServers");
    }

    public static string GetSteamCmdDirectory()
    {
        return Path.Combine(GetServiceRootPath(), "Source", "SteamCmd");
    }

    public static string GetDownloadDirectory()
    {
        return Path.Combine(GetServiceRootPath(), "Source", "Download");
    }

    public static string GetSteamCmdPath()
    {
        return GetCurrentOs() == OSPlatform.Windows ?
            Path.Combine(GetSteamCmdDirectory(), "steamcmd.exe") :
            Path.Combine(GetSteamCmdDirectory(), "steamcmd");
    }

    public static string GetSteamCachePath(string instancePath = "")
    {
        if (string.IsNullOrWhiteSpace(instancePath))
            instancePath = GetSteamCmdDirectory();

        return Path.Combine(instancePath, ".Cache");
    }

    public static string GetSteamSourcePath(string instancePath = "")
    {
        if (string.IsNullOrWhiteSpace(instancePath))
            instancePath = GetSteamCmdDirectory();
        
        return Path.Combine(instancePath, ".Source");
    }

    public static string GetSteamBackupPath(string instancePath = "")
    {
        if (string.IsNullOrWhiteSpace(instancePath))
            instancePath = GetSteamCmdDirectory();

        return Path.Combine(instancePath, ".Backup");
    }

    /// <summary>
    /// Reliable method to get IPv4 addresses based on interface type
    /// </summary>
    /// <see cref="https://stackoverflow.com/questions/6803073/get-local-ip-address"/>
    /// <param name="interfaceType">Network interface type to get IPv4 Addresses from</param>
    /// <returns></returns>
    public static IEnumerable<IPAddress> GetAllLocalIPv4(NetworkInterfaceType interfaceType)
    {
        return (from item in NetworkInterface.GetAllNetworkInterfaces()
            where item.NetworkInterfaceType == interfaceType && item.OperationalStatus == OperationalStatus.Up
            from ip in item.GetIPProperties().UnicastAddresses
            where ip.Address.AddressFamily == AddressFamily.InterNetwork
            select ip.Address).ToArray();
    }
}