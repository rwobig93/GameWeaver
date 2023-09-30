using System.Diagnostics;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using Application.Helpers;
using Domain.Models;
using Serilog;
using WeaverService.Server;

namespace WeaverService.Extensions;

public static class GameServerLocalExtensions
{
    public static bool HasRequirementsToRun(this GameServerLocal gameServerLocal)
    {
        if (!string.IsNullOrWhiteSpace(gameServerLocal.ServerProcessName) &&
            gameServerLocal.Resources.FindAll(x => x.Startup == true).Count > 0 &&
            gameServerLocal.PortGame != 0 && gameServerLocal.PortQuery != 0)
        {
            return true;
        }

        return false;
    }

    public static bool HasRequirementsToInstall(this GameServerLocal gameServerLocal)
    {
        if (gameServerLocal.Id != Guid.Empty && gameServerLocal.SteamToolId != 0)
        {
            return true;
        }

        return false;
    }

    public static bool HasRequirementsToBackup(this GameServerLocal gameServerLocal)
    {
        if (gameServerLocal.Resources.FindAll(x => x.Type == Peiskos.Shared.Enums.LocationType.SavePathRoot).Count > 0)
        {
            return true;
        }

        return false;
    }

    public static bool IsReadyToStart(this GameServerLocal gameServerLocal)
    {
        if (gameServerLocal.ServerState == ServerState.Installing ||
            gameServerLocal.ServerState == ServerState.Restarting ||
            gameServerLocal.ServerState == ServerState.Stalled ||
            gameServerLocal.ServerState == ServerState.Updating)
            return false;

        return true;
    }

    public static bool IsReadyToUpdate(this GameServerLocal gameServerLocal)
    {
        if (gameServerLocal.ServerState == ServerState.Installing ||
            gameServerLocal.ServerState == ServerState.Restarting ||
            gameServerLocal.ServerState == ServerState.Stalled ||
            gameServerLocal.ServerState == ServerState.Updating ||
            gameServerLocal.ServerState == ServerState.SpinningUp ||
            gameServerLocal.ServerState == ServerState.InternallyConnectable ||
            gameServerLocal.ServerState == ServerState.Connectable)
            return false;

        return true;
    }

    public static string GetInstallDirectory(this GameServerLocal gameServerLocal) =>
        Path.Combine(Constants.Config.PathGameServerLocation, gameServerLocal.Id.ToString());

    public static string GetFullPathFromRelative(this GameServerLocal gameServerLocal, string path) =>
        Path.Combine(Constants.Config.PathGameServerLocation, gameServerLocal.Id.ToString(), OSDynamic.UpdatePathForOS(path));

    public static string GetBackupDirectory(this GameServerLocal gameServerLocal) =>
        Path.Combine(Constants.PathBackupsFolder, gameServerLocal.Id.ToString());

    public static void UpdateFromGameProfile(this GameServerLocal gameServerLocal, GameProfileResponse gameProfile)
    {
        gameServerLocal.ServerProcessName = gameProfile.Data.ServerProcessName;
        gameServerLocal.ModList = gameProfile.Data.ModList;
        gameServerLocal.Resources = gameProfile.Data.Resources;
    }

    public static void UpdateFromGame(this GameServerLocal gameServerLocal, GameResponse responseGame)
    {
        gameServerLocal.SteamGameId = responseGame.Data.SteamGameId;
        gameServerLocal.SteamToolId = responseGame.Data.SteamToolId;
        gameServerLocal.SteamName = responseGame.Data.SteamName;
        gameServerLocal.Source = responseGame.Data.Source;
    }

    public static void UpdateFromGameServer(this GameServerLocal gameServerLocal, GameServerResponse responseGameServer)
    {
        gameServerLocal.GameDBId = responseGameServer.Data.GameId;
        gameServerLocal.GameProfileDBId = responseGameServer.Data.GameProfileId;
        gameServerLocal.ServerName = responseGameServer.Data.ServerName;
        gameServerLocal.Password = responseGameServer.Data.Password;
        gameServerLocal.PasswordRcon = responseGameServer.Data.PasswordRcon;
        gameServerLocal.PasswordAdmin = responseGameServer.Data.PasswordAdmin;
        gameServerLocal.ExtHostname = responseGameServer.Data.ExtHostname;
        gameServerLocal.PortGame = responseGameServer.Data.PortGame;
        gameServerLocal.PortQuery = responseGameServer.Data.PortQuery;
        gameServerLocal.PortRcon = responseGameServer.Data.PortRcon;
        gameServerLocal.Modded = responseGameServer.Data.Modded;

        // Connectable == Externally connectable, rely on Peiskos to tell us
        if (responseGameServer.Data.ServerState == ServerState.Connectable)
            gameServerLocal.ServerState = responseGameServer.Data.ServerState;
    }

    public static async Task UpdateInfoFromServer(this GameServerLocal gameServerLocal)
    {
        try
        {
            var responseGameServer = await API.GetGameServer(gameServerLocal.Id);
            if (responseGameServer != null)
                gameServerLocal.UpdateFromGameServer(responseGameServer);

            var responseGame = await API.GetGame(gameServerLocal.GameDBId);
            if (responseGame != null)
                gameServerLocal.UpdateFromGame(responseGame);

            var responseProfile = await API.GetGameProfile(gameServerLocal.GameProfileDBId);
            if (responseProfile != null)
                gameServerLocal.UpdateFromGameProfile(responseProfile);
            Log.Debug(
                "Updated GameServer local data from the Peiskos server: [{GameServerId}] {GameServerName}:{GameServerGamePort}:{GameServerQueryPort}",
                gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to update GameServer info from Peiskos Server: {ErrorMessage}", ex.Message);
        }
    }

    public static async Task AttemptGameProfileLocalDiscovery(this GameServerLocal gameServerLocal)
    {
        try
        {
            Log.Information(
                "Attempting Local Game Profile Discovery: [{GameServerId}] {GameServerName}:{GameServerGamePort}:{GameServerQueryPort}",
                gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
            var gameProfile = await API.GetGameProfile(gameServerLocal.GameProfileDBId);
            var currentResourceList = gameServerLocal.GetCurrentLocalResourcesForGameServer();
            var changeFound = false;
            foreach (var resource in currentResourceList)
            {
                if (gameProfile.Data.Resources.Find(x => x.Type == resource.Type && x.Path == resource.Path) is null)
                {
                    gameProfile.Data.Resources.Add(resource);
                    changeFound = true;
                }
            }

            string selectedExecutable = gameServerLocal.AttemptServerExecutableDiscovery(gameProfile);
            if (!string.IsNullOrWhiteSpace(selectedExecutable))
            {
                changeFound = true;
                gameServerLocal.ServerProcessName = selectedExecutable;
                gameProfile.Data.ServerProcessName = selectedExecutable;
            }

            if (changeFound)
            {
                var gameProfileUpdateRequest = new GameProfileRequest()
                {
                    Id = gameProfile.Data.Id,
                    GameId = gameProfile.Data.GameDBId,
                    Resources = gameProfile.Data.Resources,
                    ServerProcessName = gameProfile.Data.ServerProcessName,
                };
                await API.UpdateGameProfile(gameProfileUpdateRequest);
                Log.Information(
                    "Changes Found And Sent For Local Game Profile Discovery: [{GameServerId}] {GameServerName}:{GameServerGamePort}:{GameServerQueryPort}",
                    gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
            }

            Log.Information(
                "Finished Local Game Profile Discovery: [{GameServerId}] {GameServerName}:{GameServerGamePort}:{GameServerQueryPort}",
                gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failure occured attempting a local game profile discovery: {ErrorMessage}", ex.Message);
        }
    }

    public static bool UpdateIsAvailable(this GameServerLocal gameServerLocal)
    {
        var steamCommand =
            $"+@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +force_install_dir \"{gameServerLocal.GetInstallDirectory()}\" +login anonymous +app_info_update 1 " +
            $"+app_status {gameServerLocal.SteamToolId} +quit";
        var updateAvailable = SteamCmdHelper.CheckForAppUpdate(steamCommand);
        return updateAvailable;
    }

    public static bool InstallOrUpdateServer(this GameServerLocal gameServerLocal, bool validateFiles = false)
    {
        Log.Debug(
            "Starting GameServer Install/Update: v?[{ValidateFiles}] [{GameServerId}] {GameServerName}:{GameServerGamePort}:{GameServerQueryPort}",
            validateFiles, gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame,
            gameServerLocal.PortQuery);

        var gameServerDirectory = gameServerLocal.GetInstallDirectory();
        Directory.CreateDirectory(gameServerDirectory);

        string steamCommand;
        if (validateFiles)
            steamCommand =
                $"+@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +force_install_dir \"{gameServerDirectory}\" +login anonymous +app_info_update 1 " +
                $"+app_update {gameServerLocal.SteamToolId} +app_status {gameServerLocal.SteamToolId} validate +quit";
        else
            steamCommand =
                $"+@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +force_install_dir \"{gameServerDirectory}\" +login anonymous +app_info_update 1 " +
                $"+app_update {gameServerLocal.SteamToolId} +app_status {gameServerLocal.SteamToolId} +quit";

        var installSuccess = SteamCmdHelper.SteamCmdCommand(steamCommand, gameServerLocal);

        if (installSuccess)
        {
            Log.Information(
                "GameServer Install/Update Initiated Succeeded: v?[{ValidateFiles}] [{GameServerId}] {GameServerName}:{GameServerGamePort}:{GameServerQueryPort}",
                validateFiles, gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame,
                gameServerLocal.PortQuery);
            if (gameServerLocal.Modded)
                gameServerLocal.InstallOrUpdateModsFromWorkshop(validateFiles);
        }
        else
            Log.Information(
                "GameServer Install/Update Initiation Failed: v?[{ValidateFiles}] [{GameServerId}] {GameServerName}:{GameServerGamePort}:{GameServerQueryPort}",
                validateFiles, gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame,
                gameServerLocal.PortQuery);

        return installSuccess;
    }

    public static bool InstallOrUpdateModsFromWorkshop(this GameServerLocal gameServerLocal, bool validateFiles = false)
    {
        Log.Debug(
            "Starting GameServer Mods Install/Update: v?[{ValidateFiles}] [{GameServerId}] {GameServerName}:{GameServerGamePort}:{GameServerQueryPort}",
            validateFiles, gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame,
            gameServerLocal.PortQuery);

        if (!gameServerLocal.Modded || gameServerLocal.SteamGameId == 0)
        {
            Log.Debug("GameServer either isn't modded or is missing the SteamGameId, skipping mod install/update...");
            return false;
        }

        var modInstallSuccess = true;
        var gameServerDirectory = gameServerLocal.GetInstallDirectory();
        var gameServerSteamWorkshopDirectory = Path.Join(gameServerDirectory, "steamapps", "workshop", "content",
            gameServerLocal.SteamGameId.ToString());
        var modList = gameServerLocal.ModList.Select(x => x.SteamID).ToList();
        Directory.CreateDirectory(gameServerDirectory);

        foreach (var mod in gameServerLocal.ModList)
        {
            Log.Verbose(
                "Attempting to install Mod for GameServer: v?[{ValidateFiles}] [{GameServerId}] {GameServerName}:{ModName}:{ModSteamId}",
                validateFiles, gameServerLocal.Id, gameServerLocal.ServerName, mod.FriendlyName, mod.SteamID);

            string steamCommand;
            if (validateFiles)
                steamCommand =
                    $"+@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +force_install_dir \"{gameServerDirectory}\" +login anonymous " +
                    $"+workshop_download_item {gameServerLocal.SteamGameId} {mod.SteamID} validate +quit";
            else
                steamCommand =
                    $"+@ShutdownOnFailedCommand 1 +@NoPromptForPassword 1 +force_install_dir \"{gameServerDirectory}\" +login anonymous " +
                    $"+workshop_download_item {gameServerLocal.SteamGameId} {mod.SteamID} +quit";

            var installSuccess = SteamCmdHelper.SteamCmdCommand(steamCommand);

            if (installSuccess)
                Log.Debug(
                    "GameServer Mod Install/Update Succeeded: v?[{ValidateFiles}] [{GameServerId}] {GameServerName}:{ModName}:{ModSteamId}",
                    validateFiles, gameServerLocal.Id, gameServerLocal.ServerName, mod.FriendlyName, mod.SteamID);
            else
            {
                Log.Debug(
                    "GameServer Mod Install/Update Failed: v?[{ValidateFiles}] [{GameServerId}] {GameServerName}:{ModName}:{ModSteamId}",
                    validateFiles, gameServerLocal.Id, gameServerLocal.ServerName, mod.FriendlyName, mod.SteamID);
                modInstallSuccess = false;
            }
        }

        if (Directory.Exists(gameServerSteamWorkshopDirectory))
        {
            Log.Debug(
                "Steam workshop directory exists, attempting to remove unneeded mod directories: [{GameServerId}] {GameServerName}:{GameServerGamePort}:{GameServerQueryPort}",
                gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
            foreach (var dir in new DirectoryInfo(gameServerSteamWorkshopDirectory).EnumerateDirectories())
            {
                if (!modList.Contains(dir.Name))
                {
                    try
                    {
                        Log.Information("Deleting unneeded mod directory: {DirectoryName}", dir.Name);
                        dir.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        Log.Information(ex, "Failure occurred attempting to remove mod directory: {ErrorMessage}", ex.Message);
                    }
                }
            }
        }

        return modInstallSuccess;
    }

    public static bool UninstallServer(this GameServerLocal gameServerLocal)
    {
        var uninstallSuccess = false;
        try
        {
            Log.Debug(
                "Starting GameServer Uninstall: [{GameServerId}] {GameServerName}:{GameServerGamePort}:{GameServerQueryPort}",
                gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);

            if (Directory.Exists(gameServerLocal.GetInstallDirectory()))
            {
                Directory.Delete(gameServerLocal.GetInstallDirectory(), true);
            }

            uninstallSuccess = true;
            Log.Information(
                "GameServer Uninstall Succeeded: [{GameServerId}] {GameServerName}:{GameServerGamePort}:{GameServerQueryPort}",
                gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);

            Constants.SaveData.LocalGameServers.Remove(gameServerLocal);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failure occurred attempting to uninstall GameServer located at: {GameServerDirectory}",
                gameServerLocal.GetInstallDirectory());
        }

        return uninstallSuccess;
    }

    public static string AttemptServerExecutableDiscovery(this GameServerLocal gameServerLocal, GameProfileResponse gameProfile)
    {
        var locationPointers = gameProfile.Data.Resources.FindAll(x => x.Type == Peiskos.Shared.Enums.LocationType.Executable);
        if (locationPointers.Count == 0)
            return null;

        var selectedExecutable = "";
        var selectedPath = "";
        var highestCertainty = 0;
        foreach (var location in locationPointers)
        {
            var pointerCertainty = 0;
            var fileObject = new FileInfo(location.Path);
            if (fileObject.FullName.ToLower().Contains("binaries") || fileObject.FullName.ToLower().Contains("bin"))
                pointerCertainty += 10;
            if (fileObject.Directory.FullName == gameServerLocal.GetInstallDirectory())
                pointerCertainty += 5;
            if (fileObject.Name.ToLower().Contains("server") || fileObject.Name.ToLower().Contains("dedicated"))
                pointerCertainty += 20;
            if (fileObject.Name.ToLower().Contains("prod") || fileObject.Name.ToLower().Contains("prd"))
                pointerCertainty += 5;
            if (fileObject.Name.ToLower() == "shootergameserver")
                pointerCertainty += 5;

            if (pointerCertainty > highestCertainty)
            {
                highestCertainty = pointerCertainty;
                selectedExecutable = fileObject.Name;
                selectedPath = location.Path;
            }
        }

        gameProfile.Data.Resources.Find(x => x.Path == selectedPath).Startup = true;

        return selectedExecutable;
    }

    public static void BackupServerData(this GameServerLocal gameServerLocal)
    {
        if (!gameServerLocal.HasRequirementsToBackup())
            return;

        var backupDirectory = gameServerLocal.GetBackupDirectory();
        Directory.CreateDirectory(backupDirectory);

        var saveDataPointers = gameServerLocal.Resources.FindAll(x => x.Type == Peiskos.Shared.Enums.LocationType.SavePathRoot);
        foreach (var pointer in saveDataPointers)
        {
            if (!Directory.Exists(gameServerLocal.GetFullPathFromRelative(pointer.Path)))
                continue;

            var backupFilePrefix =
                $"{gameServerLocal.ServerName.NormalizeStringForPath()}_{pointer.Name.NormalizeStringForPath()}_";
            var newBackupArchive = Path.Combine(backupDirectory,
                $"{backupFilePrefix}{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd_HH-mm-ss")}.zip");
            if (!File.Exists(newBackupArchive))
                ZipFile.CreateFromDirectory(gameServerLocal.GetFullPathFromRelative(pointer.Path), newBackupArchive);

            if (Directory.EnumerateFiles(backupDirectory, backupFilePrefix).Count() > 50)
            {
                var extraFiles = new DirectoryInfo(backupDirectory).GetFiles(backupFilePrefix)
                    .OrderByDescending(file => file.LastWriteTime).Skip(50);
                foreach (var extraFile in extraFiles)
                {
                    try
                    {
                        extraFile.Delete();
                    }
                    catch (Exception ex)
                    {
                        Log.Information(ex, "Failed to delete old backup archive: ", ex.Message);
                    }
                }
            }
        }
    }

    public static string ParseGameServerArgs(this string arguments, GameServerLocal gameServerLocal)
    {
        return new StringBuilder(arguments)
            .Replace("%PASSWORD%", gameServerLocal.Password)
            .Replace("%RCONPASSWORD%", gameServerLocal.PasswordRcon)
            .Replace("%ADMINPASSWORD%", gameServerLocal.PasswordAdmin)
            .Replace("%SERVERNAME%", gameServerLocal.ServerName)
            .Replace("%PORTGAME%", gameServerLocal.PortGame.ToString())
            .Replace("%PORTQUERY%", gameServerLocal.PortQuery.ToString())
            .Replace("%PORTRCON%", gameServerLocal.PortRcon.ToString())
            .Replace("%IPLOCAL%", gameServerLocal.IPAddress)
            .Replace("%URLEXTERNAL%", gameServerLocal.ExtHostname)
            .ToString();
    }

    public static string NormalizeStringForPath(this string @string)
    {
        return new StringBuilder(@string)
            .Replace(" ", "").Replace("/", "").Replace("\\", "").Replace(":", "").Replace("#", "").Replace("<", "")
            .Replace(">", "")
            .Replace("$", "").Replace("!", "").Replace("`", "").Replace("&", "").Replace("*", "").Replace("\"", "")
            .Replace("'", "")
            .Replace("*", "").Replace("|", "").Replace("{", "").Replace("}", "").Replace("?", "").Replace("=", "")
            .Replace("@", "")
            .ToString();
    }

    public static string ToCamelCase(this string @string)
    {
        var x = @string.Replace("_", "");
        if (x.Length == 0) return "null";
        x = Regex.Replace(x, "([A-Z])([A-Z]+)($|[A-Z])",
            m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
        return char.ToLower(x[0]) + x.Substring(1);
    }

    public static string ToPascalCase(this string @string)
    {
        var x = @string.Replace("_", "");
        if (x.Length == 0) return "null";
        x = Regex.Replace(x, "([A-Z])([A-Z]+)($|[A-Z])",
            m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
        return char.ToUpper(x[0]) + x.Substring(1);
    }

    public static Process StartServer(this GameServerLocal gameServerLocal)
    {
        try
        {
            Log.Debug(
                "Attempting to start GameServer: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
            if (!gameServerLocal.HasRequirementsToRun())
            {
                Log.Information(
                    "GameServer doesn't have the requirements to run yet: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                    gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
                return null;
            }

            gameServerLocal.EnforceLocalConfigFiles();

            var foundProcess = gameServerLocal.GetGameServerRunningProcess();
            if (foundProcess != null)
            {
                Log.Information(
                    "Found a running process for this GameServer, not starting another one: [{ProcessId}] {ProcessName}:{ProcessPath}",
                    foundProcess.Id, foundProcess.ProcessName, foundProcess.MainModule.FileName);
                return foundProcess;
            }

            var newProcess = new Process();
            var startupResource = gameServerLocal.Resources.Find(x => x.Startup);
            newProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = gameServerLocal.GetFullPathFromRelative(startupResource.Path),
                Arguments = startupResource.Args.ParseGameServerArgs(gameServerLocal),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = gameServerLocal.GetInstallDirectory()
            };
            var processStarted = newProcess.Start();

            if (processStarted)
            {
                Log.Information(
                    "Started GameServer: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                    gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
                return newProcess;
            }
        }
        catch (Exception ex)
        {
            Log.Warning("Failed to start GameServer Process: {ErrorMessage}", ex.Message);
        }

        return null;
    }

    private static Process GetGameServerRunningProcess(this GameServerLocal gameServerLocal)
    {
        var foundProcesses = Process.GetProcessesByName(gameServerLocal.ServerProcessName);
        foreach (var process in foundProcesses)
        {
            var processFilePath = process.MainModule.FileName;
            if (processFilePath.ToLower().Contains(gameServerLocal.GetInstallDirectory().ToLower()))
            {
                Log.Verbose("Found running process from GameServer directory: [{ProcessId}] {ProcessName}", process.Id,
                    process.ProcessName);
                return process;
            }
        }

        return null;
    }

    public static bool IsGameServerRunning(this GameServerLocal gameServerLocal)
    {
        var foundProcess = gameServerLocal.GetGameServerRunningProcess();
        return foundProcess != null;
    }

    public static bool IsGameServerStalled(this GameServerLocal gameServerLocal)
    {
        var foundProcess = gameServerLocal.GetGameServerRunningProcess();
        if (foundProcess != null)
            return !foundProcess.Responding;

        return false;
    }

    public static bool StopServer(this GameServerLocal gameServerLocal)
    {
        try
        {
            Log.Debug(
                "Attempting to stop GameServer: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
            var foundProcess = gameServerLocal.GetGameServerRunningProcess();
            if (foundProcess is null)
            {
                Log.Information(
                    "Couldn't find a running process for this GameServer, can't stop it: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                    gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
                return false;
            }

            foundProcess.Kill();
            Thread.Sleep(5000);

            Log.Information(
                "Stopped running GameServer: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning("Failed to stop GameServer Process: {ErrorMessage}", ex.Message);
        }

        return false;
    }

    public static string GetFileRelativePath(this string path, GameServerLocal gameServerLocal)
    {
        var newPath = path.Replace(gameServerLocal.GetInstallDirectory(), "");
        if (!newPath.StartsWith("/"))
            newPath = $"/{newPath}";
        return newPath;
    }

    public static string GetFileAbsolutePath(this string path, GameServerLocal gameServerLocal)
    {
        return Path.Combine(gameServerLocal.GetInstallDirectory(), path);
    }

    internal static List<LocationPointer> GetCurrentLocalResourcesForGameServer(this GameServerLocal gameServerLocal)
    {
        var currentLocalResourceList = new List<LocationPointer>();

        foreach (var file in Directory.EnumerateFiles(gameServerLocal.GetInstallDirectory(), "*",
                     enumerationOptions: new EnumerationOptions() {RecurseSubdirectories = true,}))
        {
            var fileObj = new FileInfo(file);
            var extensionLower = fileObj.Extension;

            if (currentLocalResourceList.Find(x => x.Path == fileObj.FullName) is not null)
            {
                Log.Debug("File already exists in the resource gather, skipping: {FilePath}", fileObj.FullName);
                continue;
            }

            if (extensionLower.EndsWith(OSFileTypes.ExecutableWindows))
            {
                Log.Debug("Adding executable to list: {FilePath}", fileObj.FullName);
                currentLocalResourceList.Add(new LocationPointer()
                {
                    Name = fileObj.Name,
                    Path = fileObj.FullName.GetFileRelativePath(gameServerLocal),
                    Type = Peiskos.Shared.Enums.LocationType.Executable,
                    Extension = fileObj.Extension
                });
            }
            else if (extensionLower.EndsWith(OSFileTypes.ScriptWindowsCommandPrompt) ||
                     extensionLower.EndsWith(OSFileTypes.ScriptWindowsBatch))
            {
                Log.Debug("Adding script to list: {FilePath}", fileObj.FullName);
                currentLocalResourceList.Add(new LocationPointer()
                {
                    Name = fileObj.Name,
                    Path = fileObj.FullName.GetFileRelativePath(gameServerLocal),
                    Type = Peiskos.Shared.Enums.LocationType.ScriptFile,
                    Extension = fileObj.Extension
                });
            }
            else if (extensionLower.EndsWith(OSFileTypes.LogFile) || extensionLower.EndsWith(OSFileTypes.LogFileAggregate))
            {
                Log.Debug("Adding log directory to list: {FilePath}", fileObj.Directory.FullName);
                if (currentLocalResourceList.Find(x =>
                        x.Type == Peiskos.Shared.Enums.LocationType.LogPathRoot && x.Path == fileObj.DirectoryName) is null)
                {
                    currentLocalResourceList.Add(new LocationPointer()
                    {
                        Name = fileObj.Directory.Name,
                        Path = fileObj.Directory.FullName.GetFileRelativePath(gameServerLocal),
                        Type = Peiskos.Shared.Enums.LocationType.LogPathRoot
                    });
                }
            }
            else if (extensionLower.EndsWith(OSFileTypes.SavedDataSave) || extensionLower.EndsWith(OSFileTypes.SavedDataDB) ||
                     extensionLower.EndsWith(OSFileTypes.SavedDataArk))
            {
                Log.Debug("Adding save data directory to list: {FilePath}", fileObj.Directory.FullName);
                if (currentLocalResourceList.Find(x =>
                        x.Type == Peiskos.Shared.Enums.LocationType.SavePathRoot && x.Path == fileObj.DirectoryName) is null)
                {
                    currentLocalResourceList.Add(new LocationPointer()
                    {
                        Name = fileObj.Directory.Name,
                        Path = fileObj.Directory.FullName.GetFileRelativePath(gameServerLocal),
                        Type = Peiskos.Shared.Enums.LocationType.SavePathRoot
                    });
                }
            }
            else
            {
                Log.Verbose("File hit catch-all and is being ignored: {FilePath}", fileObj.FullName);
            }
        }

        return currentLocalResourceList;
    }

    private static bool EnforceConfigFileIni(this LocationPointer pointer, GameServerLocal gameServerLocal, string fullFilePath)
    {
        var enforcementSuccess = true;

        try
        {
            Log.Debug("Attempting to enforce ini config file: {FilePath}", fullFilePath);
            var iniParser = new FileIniDataParser();
            iniParser.Parser.Configuration.CaseInsensitive = false;
            iniParser.Parser.Configuration.SkipInvalidLines = true;
            iniParser.Parser.Configuration.AllowDuplicateKeys = true;
            iniParser.Parser.Configuration.OverrideDuplicateKeys = false;
            iniParser.Parser.Configuration.AllowDuplicateSections = true;
            iniParser.Parser.Configuration.AllowKeysWithoutSection = true;
            iniParser.Parser.Configuration.AllowCreateSectionsOnFly = true;
            iniParser.Parser.Configuration.ConcatenateDuplicateKeys = false;
            var originalIniData = iniParser.ReadFile(fullFilePath);
            var enforcementData = new IniData();

            foreach (var configSet in pointer.ConfigSets)
            {
                var categoryNormalized = configSet.Category.ParseGameServerArgs(gameServerLocal);
                Log.Verbose("Attempting to add config section to ini config file: [{ConfigCategory}]::{FilePath}",
                    categoryNormalized, fullFilePath);
                enforcementData.Sections.AddSection(categoryNormalized);
                foreach (var property in configSet.Properties)
                {
                    var propertyKeyNormalized = property.Key.ParseGameServerArgs(gameServerLocal).ToCamelCase();
                    var propertyValueNormalized = property.Value.ParseGameServerArgs(gameServerLocal);
                    Log.Verbose(
                        "Attempting to add config key to ini config file: [{ConfigCategory}]:{ConfigKey}:{ConfigValue}::{FilePath}",
                        categoryNormalized, propertyKeyNormalized, propertyValueNormalized, fullFilePath);
                    enforcementData[configSet.Category].AddKey(propertyKeyNormalized, propertyValueNormalized);
                }
            }

            Log.Verbose("Attempting to merge ini data structures...");
            originalIniData.Merge(enforcementData);

            Log.Verbose("Attempting to save the merged ini config file: {FilePath}", fullFilePath);
            iniParser.WriteFile(fullFilePath, originalIniData);
            Log.Debug("Successfully saved the merged ini config file: {FilePath}", fullFilePath);
        }
        catch
        {
            enforcementSuccess = false;
        }

        return enforcementSuccess;
    }

    private static bool EnforceConfigFileJson(this LocationPointer pointer, GameServerLocal gameServerLocal, string fullFilePath)
    {
        var enforcementSuccess = true;

        Log.Error("JSON Config File enforcement isn't implemented yet but was called, this was likely a mistake!");

        return enforcementSuccess;
    }

    public static void EnforceLocalConfigFiles(this GameServerLocal gameServerLocal)
    {
        Log.Debug(
            "Attempting to enforce local config files for GameServer: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
            gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);

        var configFiles = gameServerLocal.Resources.FindAll(c => c.Type == Peiskos.Shared.Enums.LocationType.ConfigFile);
        if (configFiles.Count < 1)
        {
            Log.Debug("No config files found for gameserver, skipping...");
            return;
        }

        foreach (var configFile in configFiles)
        {
            try
            {
                var configFilePath = new FileInfo(gameServerLocal.GetFullPathFromRelative(configFile.Path));
                if (!configFilePath.Exists)
                {
                    Directory.CreateDirectory(configFilePath.Directory.FullName);
                    configFilePath.Create();
                    Log.Information("Created non existing config file for GameServer: {FilePath}", configFilePath.FullName);
                }

                if (configFile.Path.ToLower().EndsWith(".json") || configFile.Extension.ToLower().EndsWith(".json"))
                {
                    if (configFile.EnforceConfigFileJson(gameServerLocal, configFilePath.FullName))
                        Log.Debug("Successfully enforced .json config file for GameServer: {FilePath}", configFilePath.FullName);
                    else
                        Log.Warning("Failed to enforce .json config file for GameServer: {FilePath}", configFilePath.FullName);
                }
                else
                {
                    if (configFile.EnforceConfigFileIni(gameServerLocal, configFilePath.FullName))
                        Log.Debug("Successfully enforced .ini config file for GameServer: {FilePath}", configFilePath.FullName);
                    else
                        Log.Warning("Failed to enforce .ini config file for GameServer: {FilePath}", configFilePath.FullName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex,
                    "Failed to enforce config file for GameServer: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery} " +
                    "| {ConfigFileName}:{ConfigFileType}:{ConfigFilePath} | Error: {ErrorMessage}",
                    gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery,
                    configFile.Name, configFile.Type, configFile.Path, ex.Message);
            }
        }
    }

    public static async Task UpdateServerState(this GameServerLocal gameServerLocal, ServerState newState)
    {
        Log.Debug(
            "Attempting to update GameServer state: {ServerStatePrevious} => {ServerStateNew} | [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
            gameServerLocal.ServerState, newState, gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame,
            gameServerLocal.PortQuery);
        gameServerLocal.ServerState = newState;

        gameServerLocal.LastStateUpdate = DateTime.Now.ToLocalTime();
        var remoteUpdateSuccess = await API.UpdateGameServerRemote(gameServerLocal.Id,
            new GameServerEditRequest() {Id = gameServerLocal.Id, ServerState = newState});

        Log.Information(
            "Remote GameServer update success?: {GameServerUpdateResult} | [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
            remoteUpdateSuccess, gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame,
            gameServerLocal.PortQuery);
    }

    public static bool IsInternallyConnectable(this GameServerLocal gameServerLocal)
    {
        Log.Verbose(
            "Attempting to validate internal connectivity for GameServer: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
            gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
        var timeout = 500;

        if (gameServerLocal.Source == GameSource.Steam)
        {
            Log.Verbose(
                "GameServer is sourced from Steam, using SteamQuery: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);

            var serverQuery = QueryMaster.GameServer.ServerQuery.GetServerInstance(EngineType.Source, "127.0.0.1",
                (ushort) gameServerLocal.PortQuery,
                sendTimeout: timeout, receiveTimeout: timeout, throwExceptions: true);
            var serverLatency = serverQuery.Ping(timeout, timeout);
            var isConnectable = serverLatency != -1;

            Log.Debug(
                "Steam sourced GameServer internally connectable?: {InternallyConnectable} => [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                isConnectable, gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame,
                gameServerLocal.PortQuery);
            return isConnectable;
        }
        else
        {
            Log.Verbose(
                "GameServer is from a manual source, using UDP & TCP connections: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
            var udpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
            foreach (var endpoint in udpListeners)
            {
                if (endpoint.Port == gameServerLocal.PortGame || endpoint.Port == gameServerLocal.PortQuery)
                {
                    Log.Debug(
                        "Manually sourced GameServer internally connectable?: {InternallyConnectable} => [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                        true, gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame,
                        gameServerLocal.PortQuery);
                    return true;
                }
            }

            var tcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            foreach (var endpoint in tcpListeners)
            {
                if (endpoint.Port == gameServerLocal.PortGame || endpoint.Port == gameServerLocal.PortQuery)
                {
                    Log.Debug(
                        "Manually sourced GameServer internally connectable?: {InternallyConnectable} => [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                        true, gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame,
                        gameServerLocal.PortQuery);
                    return true;
                }
            }
        }

        Log.Debug(
            "Manually sourced GameServer internally connectable?: {InternallyConnectable} => [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
            false, gameServerLocal.Id, gameServerLocal.ServerName, gameServerLocal.PortGame, gameServerLocal.PortQuery);
        return false;
    }
}