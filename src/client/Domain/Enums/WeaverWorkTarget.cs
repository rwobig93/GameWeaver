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
    GameServerStart = 205,
    GameServerStop = 206,
    GameServerRestart = 207,
    GameServerConfigNew = 208,
    GameServerConfigUpdate = 209,
    GameServerConfigDelete = 210,
    GameServerConfigUpdateFull = 211,
    CurrentEnd = 300
}