namespace Domain.Enums;

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
    GameServerStateUpdate = 204,
    CurrentEnd = 300
}