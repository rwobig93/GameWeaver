using Serilog;
using WeaverService.Extensions;
using WeaverService.Server;

//using Wobigtech.Core.Comm;
//using Wobigtech.Core.Enums;

namespace WeaverService.Handlers;

public static class Jobs
{
    //public static string GetCronString(CronTime time)
    //{
    //    return time switch
    //    {
    //        CronTime.MinFifteen => "*/15 * * * *",
    //        _ => "*/15 * * * *",
    //    };
    //}
    public static void GameAndModUpdater15Min()
    {
        Log.Debug("Starting GameAndModUpdater15Min()");
        // Moved functionality to webservice, this removes reliance on
        // local storage
    }

    public static void GameServerFileUpdater15Min()
    {
        Log.Debug("Starting GameServerFileUpdater15Min()");
        //Housekeeping.ReportFileChanges();
    }

    public static async Task CompanionHousekeepingCleanup()
    {
        Log.Debug("Starting general Companion housekeeping job");
        Config.Save();
        SaveData.Save();
        if (Housekeeping.CompanionHasPeiskosLogin())
        {
            await ValidateGameServerRunRequirements();
            await CheckForAvailableGameServerUpdates();
        }
        // Backup all game servers every 30min, old file cleanup included
        if (Constants.Config.LastGameServerBackup is null || DateTime.Now.ToLocalTime().AddMinutes(-30) > Constants.Config.LastGameServerBackup)
        {
            BackupGameServers();
            Constants.Config.LastGameServerBackup = DateTime.Now.ToLocalTime();
            Config.Save();
        }
    }

    private static async Task CheckForAvailableGameServerUpdates()
    {
        Log.Verbose("Attempting to check for available gameserver updates");
        foreach (var gameServer in Constants.SaveData.LocalGameServers)
        {
            try
            {
                if (gameServer.UpdateIsAvailable())
                {
                    Log.Verbose("Update is available for GameServer, attempting to add: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                        gameServer.Id, gameServer.ServerName, gameServer.PortGame, gameServer.PortQuery);

                    var updateAvailable = new Peiskos.Shared.Models.GSM.UpdateItem()
                    {
                        Name = gameServer.SteamName,
                        Source = gameServer.Source,
                        SteamId = gameServer.SteamToolId,
                    };
                    gameServer.UpdatesWaiting.Add(updateAvailable);

                    await API.UpdateGameServerRemote(gameServer.Id, new Peiskos.Shared.Requests.GSM.GameServerEditRequest()
                    {
                        Id = gameServer.Id,
                        UpdatesWaiting = gameServer.UpdatesWaiting,
                    });

                    Log.Debug("Added available game update: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery} => [{UpdateSource}]{UpdateName}:{UpdateSteamId}",
                        gameServer.Id, gameServer.ServerName, gameServer.PortGame, gameServer.PortQuery, updateAvailable.Source, updateAvailable.Name, updateAvailable.SteamId);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error occured attempting to check for available GameServer updates: {ErrorMessage}", ex.Message);
            }
        }
    }

    public static async Task GameServerWatcher()
    {
        Log.Verbose("Starting GameServerWatcher");
        foreach (var gameServer in Constants.SaveData.LocalGameServers)
        {
            if (gameServer.IsGameServerStalled() && gameServer.ServerState != Peiskos.Shared.Enums.ServerState.Stalled)
            {
                await gameServer.UpdateServerState(Peiskos.Shared.Enums.ServerState.Stalled);
                continue;
            }

            var serverRunning = gameServer.IsGameServerRunning();
            if (serverRunning && gameServer.ServerState == Peiskos.Shared.Enums.ServerState.Connectable)
                continue;

            if (!serverRunning && gameServer.ServerState == Peiskos.Shared.Enums.ServerState.Shutdown)
                continue;

            if (!serverRunning && gameServer.ServerState != Peiskos.Shared.Enums.ServerState.Shutdown)
            {
                await gameServer.UpdateServerState(Peiskos.Shared.Enums.ServerState.Shutdown);
                continue;
            }

            var internallyConnectable = gameServer.IsInternallyConnectable();
            if (internallyConnectable && 
                (gameServer.ServerState != Peiskos.Shared.Enums.ServerState.InternallyConnectable || 
                gameServer.ServerState != Peiskos.Shared.Enums.ServerState.Connectable))
            {
                await gameServer.UpdateServerState(Peiskos.Shared.Enums.ServerState.InternallyConnectable);
                continue;
            }

            if (gameServer.LastStateUpdate.AddMinutes(10) > DateTime.Now.ToLocalTime() &&
                (gameServer.ServerState == Peiskos.Shared.Enums.ServerState.Installing ||
                gameServer.ServerState == Peiskos.Shared.Enums.ServerState.Updating ||
                gameServer.ServerState == Peiskos.Shared.Enums.ServerState.Restarting ||
                gameServer.ServerState == Peiskos.Shared.Enums.ServerState.SpinningUp))
            {
                Log.Debug("Server is installing, updating, restarting or spinning up within 10min, skipping...: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                    gameServer.Id, gameServer.ServerName, gameServer.PortGame, gameServer.PortQuery);
                continue;
            }

            Log.Warning("Unknown state hit for GameServer via the watcher: [{GameServerId}] {GameServerName}:{GameServerPortGame}:{GameServerPortQuery}",
                    gameServer.Id, gameServer.ServerName, gameServer.PortGame, gameServer.PortQuery);
            await gameServer.UpdateServerState(Peiskos.Shared.Enums.ServerState.Unknown);
        }
    }

    private static async Task ValidateGameServerRunRequirements()
    {
        foreach (var gameServer in Constants.SaveData.LocalGameServers)
        {
            if (!gameServer.HasRequirementsToRun() || !gameServer.HasRequirementsToInstall() || !gameServer.HasRequirementsToBackup())
            {
                await gameServer.UpdateInfoFromServer();
            }
        }
    }

    private static void BackupGameServers()
    {
        Log.Verbose("Attempting to backup all gameservers");
        foreach (var gameServer in Constants.SaveData.LocalGameServers)
            gameServer.BackupServerData();
        Log.Debug("Successfully backed up all gameservers");
    }
}
