﻿using Application.Models.GameServer;
using Domain.Contracts;
using Domain.Enums;
using Domain.Models.GameServer;

namespace Application.Services;

public interface IGameServerService
{
    public static bool SteamCmdUpdateInProgress { get; private set; }
    Task<IResult> ValidateSteamCmdInstall();
    void ClearSteamCmdData();
    Task<IResult<Guid>> Create(GameServerLocal gameServer);
    Task<IResult<GameServerLocal?>> GetById(Guid id);
    Task<IResult<List<GameServerLocal>>> GetAll();
    Task<IResult> InstallOrUpdateGame(Guid id, bool validate = false);
    Task<IResult> InstallOrUpdateMod(Guid id, Mod mod);
    Task<IResult> UninstallGame(Guid id);
    Task<IResult> UninstallMod(Guid id, Mod mod);
    Task<IResult> BackupGame(Guid id);
    Task<IResult> StartServer(Guid id);
    Task<IResult> StopServer(Guid id);
    Task<IResult> Update(GameServerLocalUpdate gameServerUpdate);
    Task<IResult> UpdateState(Guid id, ServerState state);
    Task<IResult> UpdateState(Guid id, ServerState state, string storageConfigHash);
    Task<IResult> UpdateState(Guid id, ServerState state, string runningConfigHash, string storageConfigHash);
    Task<IResult> Housekeeping();
    Task<IResult<ServerState>> GetCurrentRealtimeState(Guid id);
    Task<IResult> UpdateConfigItemFiles(Guid id, bool loadExisting = true);
}