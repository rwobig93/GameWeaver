using Serilog;

namespace Application.Helpers;

public static class ThreadHelper
{
    public static void ConfigureThreadPool(int minimumThreads, int maximumThreads)
    {
        Log.Debug("Setting minimum and maximum threads: [Min}{MinimumThreads} [Max]{MaximumThreads}", minimumThreads, maximumThreads);
        ThreadPool.SetMinThreads(minimumThreads, minimumThreads);
        ThreadPool.SetMaxThreads(maximumThreads, maximumThreads);
        Log.Information("Updated minimum and maximum threads: [Min}{MinimumThreads} [Max]{MaximumThreads}", minimumThreads, maximumThreads);
    }

    public static void QueueWork(WaitCallback work)
    {
        Log.Debug("Queuing work {CallbackName} to the thread pool", nameof(work));
        ThreadPool.QueueUserWorkItem(work);
    }
}