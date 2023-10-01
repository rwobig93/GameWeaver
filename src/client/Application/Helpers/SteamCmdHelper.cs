using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using Domain.Models;
using Serilog;

namespace Application.Helpers;

public static class SteamCmdHelper
{
    private static bool SteamCmdCommand(string command, GameServerLocal? gameServerLocal = null,
        DataReceivedEventHandler? outputHandler = null, EventHandler? exitHandler = null)
    {
        // .\steamcmd.exe +login anonymous +force_install_dir "ConanExiles" +app_update 443030 validate +quit
        // .\steamcmd.exe +@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +login anonymous +force_install_dir "ConanExiles" +app_info_update 1 +app_update 443030 +app_status 443030 +quit
        // .\steamcmd.exe +@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +login ${SteamLoginName} +app_info_update 1 +app_status ${SteamAppID} +quit/
        // https://steamapi.xpaw.me/
        Log.Verbose("Starting SteamCMDCommand({Command}, {OutputHandler}, {ExitHandler})", command, outputHandler, exitHandler);
        try
        {
            // Steam CMD will update by default w/o an argument
            if (command == "update")
                command = "";

            // Add +quit to the end of the argument array to close upon finishing
            if (!command.EndsWith("+quit"))
                command += " +quit";

            // var steamDto = new RunSteamDto()
            // {
            //     Command = command,
            //     OutputHandler = outputHandler,
            //     ExitHandler = exitHandler,
            //     GameServer = gameServerLocal
            // };

            // TODO: Look at ThreadRunner options
            // var threadAdded = ThreadRunner.SteamCMDRun(RunSteamCmd, steamDto);
            //
            // if (threadAdded)
            //     Log.Verbose("Thread Successfully Added to ThreadQueue");
            // else
            //     Log.Warning("Thread add failed and wasn't added to ThreadQueue");
            //
            // Log.Verbose($"Ran SteamCMD Command: {command}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("SteamCMDCommand Error: {ExMessage}", ex.Message);
            return false;
        }
    }

    public static bool CheckForAppUpdate(string command)
    {
        // .\steamcmd.exe +@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +login anonymous +force_install_dir "ConanExiles" +app_info_update 1 +app_status "443030" +quit
        // See: https://steamcommunity.com/app/346110/discussions/0/535152511358957700/#c1768133742959565192
        Log.Verbose("Starting CheckForAppUpdate({Command})", command);
        try
        {
            // Add +quit to the end of the argument array to close upon finishing
            if (!command.EndsWith("+quit"))
                command += " +quit";

            var steamCmd = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = OsHelper.GetSteamCmdPath(),
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            // steamCmd.OutputDataReceived += Events.SteamCMD_OutputDataReceived;
            // steamCmd.Exited += Events.SteamCMD_Exited;
            steamCmd.Start();

            var installState = HandleSteamCmdOutput(steamCmd);
            Log.Debug($"STEAMCMD_CHECK_UPDATE: App install state: {installState.Replace("- install state: ", "")}");
            if (installState.ToLower().Contains("update required"))
                return true;

            Log.Verbose($"Ran SteamCMD Command: {command}");
            return false;
        }
        catch (Exception ex)
        {
            Log.Error($"SteamCMDCommand Error: {ex.Message}");
            return false;
        }
    }

    private static void RunSteamCmd(RunSteamDto runSteamDto)
    {
        Log.Verbose("Starting RunSteamCMD");

        var steamCmd = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = OsHelper.GetSteamCmdPath(),
                Arguments = runSteamDto.Command,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        if (runSteamDto.OutputHandler != null)
        {
            Log.Verbose("outputHandler wasn't null, using specified callback");
            steamCmd.OutputDataReceived += runSteamDto.OutputHandler;
        }
        else
        {
            Log.Verbose("outputHandler was null, using default event handler");
            // steamCmd.OutputDataReceived += Events.SteamCMD_OutputDataReceived;
        }

        if (runSteamDto.GameServer != null)
        {
            Log.Verbose("GameServer wasn't null, using async callback for GameServer update on exit");
            // steamCmd.Exited += async (s, e) => await Events.SteamCMD_Exit_UpdateGameServerState(s, e, runSteamDto.GameServer);
        }
        else if (runSteamDto.ExitHandler != null)
        {
            Log.Verbose("exitHandler wasn't null, using specified callback");
            steamCmd.Exited += runSteamDto.ExitHandler;
        }
        else
        {
            Log.Verbose("exitHandler was null, using default event handler");
            // steamCmd.Exited += Events.SteamCMD_Exited;
        }

        steamCmd.Start();

        HandleSteamCmdOutput(steamCmd);

        Log.Verbose("Finished RunSteamCMD");
    }

    private static string HandleSteamCmdOutput(Process process)
    {
        // TODO: Look at alternatives for service injection with server return i/o for server consumption and client separation
        var input = process.StandardInput;
        var output = process.StandardOutput;
        var errors = process.StandardError;

        try
        {
            while (!process.HasExited)
            {
                var received = output.ReadLine();
                if (received != null && received.ToLower().Contains("install state:"))
                    return received;
                Log.Verbose($"STEAMCMD_OUTPUT: {received}");
            }

            Log.Verbose("STEAMCMD_OUTPUT_ENDED");
            return "";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failure occured during SteamCMD Output Handling");
            return "";
        }
        finally
        {
            input.Flush();
            input.Close();
            output.Close();
            errors.Close();
        }
    }

    public static void ResetSteamCmdToFresh()
    {
        Log.Verbose("Starting ResetSteamCMDToFresh()");
        var steamDir = new DirectoryInfo(OsHelper.GetSteamCmdDirectory());
        foreach (var file in steamDir.EnumerateFiles())
        {
            try
            {
                if (file.Name.ToLower() == "steamcmd.exe")
                {
                    Log.Verbose($"Skipping steamcmd.exe: {file.FullName}");
                }
                else
                {
                    Log.Debug($"Deleting File: {file.FullName}");
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                Log.Information(ex, "Failure when attempting to clean file: {FileError}", ex.Message);
            }
        }

        foreach (var dir in steamDir.EnumerateDirectories())
        {
            Log.Debug($"Deleting directory recursively: {dir.FullName}");
            dir.Delete(true);
        }

        // TODO: Validate Log for static class usage vs. service that would be called by a Singleton service
        Log.Information($"Finished resetting steam directory to fresh: {OsHelper.GetSteamCmdDirectory()}");
    }

    public static void CleanupCache()
    {
        // TODO: Remap paths based on instance, validate source or aggregation
        Log.Verbose($"Starting CleanupCache() at {OsHelper.GetSteamCachePath()}");
        DirectoryInfo cacheFolder = new DirectoryInfo(OsHelper.GetSteamCachePath());
        foreach (FileInfo file in cacheFolder.EnumerateFiles())
        {
            Log.Debug($"Deleting file: {file.FullName}");
            file.Delete();
        }

        foreach (DirectoryInfo dir in cacheFolder.EnumerateDirectories())
        {
            Log.Debug($"Deleting directory recursively: {dir.FullName}");
            dir.Delete(true);
        }

        Log.Information($"Finished cleaning up cache folder: {OsHelper.GetSteamCachePath()}");
    }

    public static void ExtractSteamCmd()
    {
        Log.Verbose("Starting ExtractSteamCMD()");
        var steamCmdDir = OsHelper.GetSteamCmdDirectory();
        if (Directory.Exists(steamCmdDir))
        {
            try
            {
                Log.Warning($"Somehow steamcmd exists when we already checked for it so we'll delete it: {steamCmdDir}");
                Log.Debug($"Deleting existing steamcmd directory: {steamCmdDir}");
                Directory.Delete(steamCmdDir, true);
                Log.Information($"Deleted existing steamcmd directory: {steamCmdDir}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete existing steamcmd directory before extraction");
            }
        }

        if (!Directory.Exists(steamCmdDir))
        {
            try
            {
                Log.Debug($"Attempting to create missing steam directory: {steamCmdDir}");
                Directory.CreateDirectory(steamCmdDir);
                Log.Information($"Created missing steam directory: {steamCmdDir}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create missing steam directory for archive extraction");
            }
        }

        try
        {
            ZipFile.ExtractToDirectory(Path.Combine(OsHelper.GetSteamCachePath(), "steamcmd.zip"), steamCmdDir);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unable to extract steamcmd archive");
        }

        Log.Information("Finished extracting SteamCMD to source folder");
    }

    public static void DownloadSteamCmd()
    {
        Log.Verbose("Starting DownloadSteamCMD()");
        // TODO: Convert stack methods to async and move to HttpClient instead of WebClient
        // using (var httpClient = new HttpClient())
        // {
        //     var responseStream = await httpClient.GetStreamAsync(Constants.URLSteamCMDDownload);
        //     using var fileStream = new FileStream(Path.Combine(Constants.PathCache, "steamcmd.zip"), FileMode.Create);
        //     await responseStream.CopyToAsync(fileStream);
        // }
#pragma warning disable SYSLIB0014
        using (var webClient = new WebClient())
#pragma warning restore SYSLIB0014
        {
            // webClient.DownloadProgressChanged += Events.WebClient_DownloadProgressChanged;
            // webClient.DownloadFileCompleted += Events.WebClient_DownloadFileCompleted;
            webClient.DownloadFile(OsHelper.GetDownloadDirectory(), Path.Combine(OsHelper.GetSteamCachePath(), "steamcmd.zip"));
        }

        Log.Debug("Finished downloading SteamCMD");
    }

    public static void UpdateSteamCmd()
    {
        Log.Verbose("Starting UpdateSteamCMD()");
        SteamCmdHelper.SteamCmdCommand("update");
        Log.Debug("Started SteamCMD Update");
    }
}