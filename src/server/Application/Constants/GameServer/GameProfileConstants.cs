namespace Application.Constants.GameServer;

public static class GameProfileConstants
{
    public const string ServerProfileNamePrefix = "Server Profile -";
    public const string GameProfileDefaultNamePrefix = "Game Profile -";
    public const string EmptyProfileNamePrefix = "Profile -";

    public static readonly string[] InvalidProfileNames = [ServerProfileNamePrefix, GameProfileDefaultNamePrefix];
}