using Domain.Enums.GameServer;

namespace Application.Helpers.GameServer;

public static class ConnectivityStateHelpers
{
    public static bool IsRunning(this ConnectivityState connectivityState)
    {
        return connectivityState switch
        {
            ConnectivityState.Unknown or ConnectivityState.UnRegistered or ConnectivityState.Unreachable or ConnectivityState.Shutdown or ConnectivityState.Uninstalled
                or ConnectivityState.OverlappingPort => false,

            ConnectivityState.SpinningUp or ConnectivityState.Connectable or ConnectivityState.InternallyConnectable or ConnectivityState.Updating or ConnectivityState.Stalled
                or ConnectivityState.Restarting or ConnectivityState.Installing or ConnectivityState.Uninstalling or ConnectivityState.ShuttingDown
                or ConnectivityState.Discovering => true,

            _ => throw new ArgumentOutOfRangeException(nameof(connectivityState), connectivityState, null)
        };
    }

    public static bool IsDoingSomething(this ConnectivityState connectivityState)
    {
        return connectivityState switch
        {
            ConnectivityState.Updating or ConnectivityState.Discovering or ConnectivityState.Installing or ConnectivityState.Restarting or ConnectivityState.Uninstalling
                or ConnectivityState.ShuttingDown => true,

            _ => false
        };
    }
}