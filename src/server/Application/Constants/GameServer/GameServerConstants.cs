namespace Application.Constants.GameServer;

public static class GameServerConstants
{
    public const string NoAccessValue = "<Redacted-For-No-Access>";
    public static readonly int[] ConnectableCheckAttempts = [1000, 1000, 1000, 2000, 2000, 3000, 5000];  // 7 Attempts over 15 seconds
}