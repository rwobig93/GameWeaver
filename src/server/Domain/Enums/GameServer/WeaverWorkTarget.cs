namespace Domain.Enums.GameServer;

public enum WeaverWorkTarget
{
    StatusUpdate = 001,
    Host = 100,
    HostStatusUpdate = 101,
    HostDetail = 102,
    GameServer = 200,
    GameServerInstall = 201,
    GameServerUpdate = 202,
    GameServerUninstall = 203,
    CurrentEnd = 300
}