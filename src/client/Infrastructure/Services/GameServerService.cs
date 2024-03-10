using System.Diagnostics;
using System.IO.Compression;
using Application.Constants;
using Application.Helpers;
using Application.Services;
using Domain.Contracts;
using Domain.Models.GameServer;

namespace Infrastructure.Services;

public class GameServerService : IGameServerService
{
    private readonly ILogger _logger;
    private readonly IDateTimeService _dateTimeService;

    public GameServerService(ILogger logger, IDateTimeService dateTimeService)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public static bool SteamCmdUpdateInProgress { get; private set; }

    public async Task<IResult> ValidateSteamCmdInstall()
    {
        var steamCmdDir = OsHelper.GetSteamCmdDirectory();
        var steamCmdPath = OsHelper.GetSteamCmdPath();
        var steamCmdDownloadPath = Path.Combine(OsHelper.GetSteamCachePath(), "steamcmd.zip");
        
        if (!File.Exists(steamCmdPath))
        {
            SteamCmdUpdateInProgress = true;
            _logger.Information("SteamCMD doesn't exist, installing fresh");
            _logger.Debug("SteamCMD path: {File}", steamCmdPath);
            
            var downloadResult = await DownloadSteamCmd(steamCmdDownloadPath);
            if (!downloadResult.Succeeded)
            {
                SteamCmdUpdateInProgress = false;
                return downloadResult;
            }
            
            var extractResult = await ExtractSteamCmdArchive(steamCmdDownloadPath, steamCmdDir);
            if (!extractResult.Succeeded)
            {
                SteamCmdUpdateInProgress = false;
                return extractResult;
            }

            SteamCmdUpdateInProgress = false;
            _logger.Information("Successfully installed SteamCMD at {Path}", steamCmdPath);
        }
        
        _logger.Debug("SteamCMD is installed, checking for updates...");
        SteamCmdUpdateInProgress = true;
        var updateResult = await RunSteamCmdCommand(SteamConstants.CommandUpdate, true);
        if (!updateResult.Succeeded)
            return updateResult;

        return await Result.SuccessAsync();
    }

    private async Task<IResult> DownloadSteamCmd(string downloadPath)
    {
        try
        {
            if (File.Exists(downloadPath))
            {
                _logger.Debug("Existing SteamCMD download exists, deleting before downloading: {FilePath}", downloadPath);
                File.Delete(downloadPath);
            }

            var downloadDirectory = new FileInfo(downloadPath).DirectoryName ?? OsHelper.GetSteamCachePath();
            Directory.CreateDirectory(downloadDirectory);
            _logger.Debug("SteamCMD download directory: {Directory}", downloadDirectory);
            using (var httpClient = new HttpClient())
            {
                var responseStream = await httpClient.GetStreamAsync(SteamConstants.SteamCmdDownloadUrl);
                await using var fileStream = new FileStream(downloadPath, FileMode.Create);
                await responseStream.CopyToAsync(fileStream);
            }

            return await Result.SuccessAsync($"Successfully downloaded SteamCMD to: {downloadPath}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred downloading SteamCMD: {Error}", ex.Message);
            return await Result.FailAsync($"Failed to download SteamCMD: {ex.Message}");
        }
    }

    private async Task<IResult> ExtractSteamCmdArchive(string archivePath, string destinationPath)
    {
        try
        {
            Directory.CreateDirectory(destinationPath);
            _logger.Debug("Created SteamCMD directory for archive extraction: {Directory}", destinationPath);
            
            ZipFile.ExtractToDirectory(archivePath, destinationPath);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to extract SteamCMD archive: {Error}", ex.Message);
            return await Result.FailAsync($"Failed to extract SteamCMD archive: {ex.Message}");
        }
    }

    private async Task<IResult> RunSteamCmdCommand(string command, bool isMaintenanceCommand = false)
    {
        _logger.Debug("Running SteamCMD command: [Maintenance:{IsMaintenance}]{Command}", isMaintenanceCommand, command);
        try
        {
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

            steamCmd.Start();

            HandleSteamCmdOutput(steamCmd, isMaintenanceCommand);
            _logger.Debug("Finished running SteamCMD command: {Command}", command);
            return await Result.SuccessAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occurred running SteamCMD command: {Error}", ex.Message);
            return await Result.FailAsync($"Failure occurred running SteamCMD command: {ex.Message}");
        }
    }

    private void HandleSteamCmdOutput(Process process, bool commandIsMaintenance)
    {
        var input = process.StandardInput;
        var output = process.StandardOutput;
        var errors = process.StandardError;

        try
        {
            while (!process.HasExited)
            {
                var received = output.ReadLine();
                if (received is not null && received.ToLower().Contains("install state:"))
                    return;
                
                _logger.Verbose("STEAMCMD_OUTPUT[{ProcessId}]: {SteamCmdReceived}", process.Id, received);
            }

            _logger.Verbose("STEAMCMD_OUTPUT_ENDED[{ProcessId}]", process.Id);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failure occured during SteamCMD Output Handling: {Error}", ex.Message);
        }
        finally
        {
            input.Flush();
            input.Close();
            output.Close();
            errors.Close();

            if (commandIsMaintenance)
                SteamCmdUpdateInProgress = false;
        }
    }
    
    public async Task<IResult<SoftwareUpdateStatus>> CheckForUpdateGame()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult<SoftwareUpdateStatus>> CheckForUpdateMod()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> InstallOrUpdateGame()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> InstallOrUpdateMod()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> UninstallGame()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> UninstallMod()
    {
        throw new NotImplementedException();
    }
}