using System.ComponentModel;
using Domain.Models;
using Serilog;

namespace WeaverService.Handlers;

public class ThreadRunner
{
    private static BackgroundWorker GameRunner { get; set; }
    private static List<RunSteamThreadDto> QueueGameRunner { get; set; }
    private static bool RunningGameRunner = false;

    public static bool SteamCMDRun(Action<RunSteamDto> callbackMethod, RunSteamDto steamDto)
    {
        try
        {
            if (QueueGameRunner == null)
            {
                InitializeQueueGameRunner();
            }
            if (GameRunner == null)
            {
                InitializeThreadRunner();
            }

            AddThreadedMethodToQueue(new RunSteamThreadDto()
            {
                RunSteamMethod = callbackMethod,
                SteamDto = steamDto
            });

            return true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to Add Threaded Method to ThreadQueue");
            return false;
        }
    }

    private static void AddThreadedMethodToQueue(RunSteamThreadDto steamThreadDto)
    {
        Log.Debug("Starting AddThreadedMethodToQueue");
        QueueGameRunner.Add(steamThreadDto);
        Log.Debug("Finished AddThreadedMethodToQueue");
    }

    private static void InitializeQueueGameRunner()
    {
        Log.Debug("Running InitializeQueueGameRunner()");
        QueueGameRunner = new List<RunSteamThreadDto>();
        Log.Information("Initialized new GameRunner Queue");
    }

    private static void InitializeThreadRunner()
    {
        Log.Debug("Running InitializeThreadRunner()");

        RunningGameRunner = true;
        GameRunner = new BackgroundWorker
        {
            WorkerReportsProgress = true
        };
        GameRunner.ProgressChanged += GameRunner_ProgressChanged;
        GameRunner.DoWork += GameRunner_DoWork;
        GameRunner.RunWorkerCompleted += GameRunner_RunWorkerCompleted;
        GameRunner.RunWorkerAsync();

        Log.Information("Initialized new GameRunner");
    }

    private static void GameRunner_DoWork(object sender, DoWorkEventArgs e)
    {
        Log.Debug("Running GameRunner_DoWork");
        while (RunningGameRunner)
        {
            try
            {
                if (QueueGameRunner.Count <= 0)
                {
                    Thread.Sleep(500);
                }
                else
                {
                    QueueGameRunner[0].RunSteamMethod(QueueGameRunner[0].SteamDto);
                    QueueGameRunner.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure occured on GameRunner Queue");
            }
        }
        Log.Debug("Finished GameRunner_DoWork");
    }

    private static void GameRunner_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        Log.Debug($"GameRunner Progress Change: {e.ProgressPercentage}");
    }

    private static void GameRunner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        Log.Information("GameRunner Thread Finished");
    }
}
