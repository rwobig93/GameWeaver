using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Domain.Enums;
using Domain.Models.Network;
using Serilog;

namespace Application.Helpers;

public static class OsHelpers
{
    public static string GetServiceRootPath()
    {
        return Directory.GetCurrentDirectory();
    }

    public static OsType GetCurrentOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return OsType.Windows;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OsType.Linux;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            return OsType.Linux;
        
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OsType.Mac : OsType.Unknown;
    }

    public static string GetConfigPath()
    {
        return Path.Combine(GetServiceRootPath(), "Config");
    }

    public static string GetDefaultGameServerPath()
    {
        return Path.Combine(GetServiceRootPath(), "GameServers");
    }

    public static string GetDefaultBackupPath()
    {
        return Path.Combine(GetServiceRootPath(), "Backups");
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
        return GetCurrentOs() == OsType.Windows ?
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

    public static string GetDebugSanitizedPath(string debugPath)
    {
        var pathParts = debugPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        var binIndex = Array.IndexOf(pathParts, "bin");

        if (binIndex < 0)
        {
            throw new InvalidOperationException("The 'bin' directory was not found in the path.");
        }

        var projectPathParts = pathParts.Take(binIndex);
        return Path.Combine(projectPathParts.ToArray());
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

    public static IEnumerable<NetworkListeningSocket> GetListeningSockets()
    {
        var networkListeners = new List<NetworkListeningSocket>();

        foreach (var tcpSocket in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
        {
            try
            {
                networkListeners.Add(new NetworkListeningSocket
                {
                    Protocol = ProtocolType.Tcp,
                    Port = tcpSocket.Port
                });
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to parse TCP socket listener: {Error}", ex.Message);
            }
        }

        foreach (var udpSocket in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners())
        {
            try
            {
                networkListeners.Add(new NetworkListeningSocket
                {
                    Protocol = ProtocolType.Udp,
                    Port = udpSocket.Port
                });
            }
            catch (Exception ex)
            {
                Log.Debug("Failed to parse UDP socket listener: {Error}", ex.Message);
            }
        }

        return networkListeners;
    }

    public static IEnumerable<Process> GetProcessesByDirectory(string directoryPath)
    {
        var matchingProcesses = new List<Process>();
        var allProcesses = Process.GetProcesses();

        foreach (var process in allProcesses)
        {
            try
            {
                // Get the full path of the process executable
                var processPath = process.MainModule?.FileName;
                if (processPath is null)
                {
                    Log.Information("Unable to get process module for path validation: [{ProcessId}]{ProcessName}", process.Id, process.ProcessName);
                    continue;
                }

                // Normalize the paths to ensure case-insensitivity and trailing separators do not affect comparison
                var fullProcessPath = Path.GetFullPath(processPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var fullDirectoryPath = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                // Check if the process's path starts with the directory path
                if (fullProcessPath.StartsWith(fullDirectoryPath, StringComparison.OrdinalIgnoreCase))
                    matchingProcesses.Add(process);
            }
            catch (Exception ex)
            {
                // Exceptions are expected when accessing certain process properties (e.g., access denied)
                Log.Debug("Could not access path for process [{ProcessId}]{ProcessName}: {ErrorMessage}",
                    process.Id, process.ProcessName, ex.Message);
            }
        }

        return matchingProcesses;
    }
}